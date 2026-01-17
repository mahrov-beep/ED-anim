namespace Game.ServerRunner.Grains;

using System.Buffers;
using System.Linq;
using Core;
using Db;
using Db.Model;
using JetBrains.Annotations;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using Multicast;
using Multicast.ServerData;
using Orleans.Concurrency;
using Quantum;
using Shared.DTO;
using Shared.ServerEvents;
using Shared.UserProfile;
using Shared.UserProfile.Commands;
using Shared.UserProfile.Data;

public interface IUserProfileGrain : IGrainWithGuidKey {
    ValueTask<byte[]> GetSerializedUserProfile();

    ValueTask<(ServerCommandResultCode code, string error)> Execute(IUserProfileServerCommand command, UserProfileGrainExecuteOptions options);

    [OneWay] ValueTask PublishAppServerEvent(IAppServerEvent evt);

    ValueTask<ServerResult<UserDeleteResponse>> DeleteUserProfile(UserDeleteRequest request);

    ValueTask<Quantum.GameSnapshotLoadout> GetSelectedLoadoutSnapshot();

    ValueTask<string> GetNickName();

    ValueTask<int> GetLevel();
}

[Flags]
public enum UserProfileGrainExecuteOptions {
    None = 0,
    SendUserProfileUpdatedEvent = 1 << 0,
}

public static class UserProfileGrainExtensions {
    public static Guid GetUserId(this IUserProfileGrain grain) => grain.GetPrimaryKey();

    public static IUserProfileGrain GetUserProfileGrain(this IGrainFactory grainFactory, Guid userId) => grainFactory.GetGrain<IUserProfileGrain>(userId);
}

public class UserProfileGrain : Grain, IUserProfileGrain {
    private static readonly MessagePackSerializerOptions MsgPackServerDataOptions = MessagePackSerializer.DefaultOptions
        .WithCompression(MessagePackCompression.Lz4BlockArray);

    private readonly ServerCommandHandlerRegistry<UserProfileServerCommandContext, SdUserProfile> commandHandlerRegistry;

    private readonly ILogger<UserProfileGrain>        logger;
    private readonly UserProfileServerCommandContext  commandContext;
    private readonly IDbContextFactory<GameDbContext> dbContextFactory;
    private readonly SdUserProfile                    userProfile;
    private readonly ArrayBufferWriter<byte>          userProfileBuffer;

    private Guid UserId => this.GetUserId();

    public UserProfileGrain(
        ILogger<UserProfileGrain> logger,
        ServerCommandHandlerRegistry<UserProfileServerCommandContext, SdUserProfile> commandHandlerRegistry,
        IDbContextFactory<GameDbContext> dbContextFactory) {
        this.logger                 = logger;
        this.commandContext         = new GrainUserProfileServerCommandContext(this.ExecuteInternal, this.AsReference<IUserProfileGrain>());
        this.commandHandlerRegistry = commandHandlerRegistry;
        this.dbContextFactory       = dbContextFactory;
        this.userProfile            = SdUserProfile.Create();
        this.userProfileBuffer      = new ArrayBufferWriter<byte>();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken) {
        await base.OnActivateAsync(cancellationToken);

        this.logger.LogInformation("Activate (UserId={UserId})",
            this.UserId);

        await this.ReadUserProfileFromDb(createIfNotExist: true);

        await this.Execute(new UserProfileActivateCommand(), UserProfileGrainExecuteOptions.None);
    }

    public async ValueTask<(ServerCommandResultCode code, string error)> Execute(IUserProfileServerCommand command, 
        UserProfileGrainExecuteOptions options) {
        ArgumentNullException.ThrowIfNull(command);

        this.logger.LogInformation("Execute (UserId={UserId}, CommandType={CommandType})",
            this.UserId, command.GetType().Name);

        try {
            var wasPlaying = this.userProfile.PlayedGames.Any(g => g.IsPlaying.Value);
            var result = await this.ExecuteInternal(command);
            var isPlaying = this.userProfile.PlayedGames.Any(g => g.IsPlaying.Value);

            if (!wasPlaying && isPlaying) {
                await this.GrainFactory.GetGrain<IUserStatusGrain>(this.UserId).EnterGame();
                
                var leaderId = await this.GrainFactory.GetUserPartyGrain(this.UserId).GetLeader();
                if (leaderId != Guid.Empty) {
                    await this.GrainFactory.GetPartyGrain(leaderId).OnMemberEnteredGame(this.UserId);
                }
            }
            else if (wasPlaying && !isPlaying) {
                await this.GrainFactory.GetGrain<IUserStatusGrain>(this.UserId).BackToMenu();
            }

            await this.WriteUserProfileToDb();

            if ((options & UserProfileGrainExecuteOptions.SendUserProfileUpdatedEvent) != 0) {
                await this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
                    .GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(this.GetUserId()))
                    .OnNextAsync(new UserProfileUpdatedAppServerEvent());
            }

            return (result.ResultCode, result.Error);
        }
        catch (ServerCommandResultException resultException) when (resultException.Result.ResultCode == ServerCommandResultCode.BadRequest) {
#if DEBUG || true
            var debugHandlerChain = string.Join("->", (resultException.StackTrace?.Split('\n') ?? [])
                .Select(it => it.Contains("CommandHandler") ? it.Split('.').FirstOrDefault(p => p.EndsWith("CommandHandler")) : null)
                .Where(it => it != null).Reverse());

            var error = $"{debugHandlerChain} :: {resultException.Result.Error}";
#else
            var error = resultException.Result.Error;
#endif

            await this.ReadUserProfileFromDb(createIfNotExist: false);

            return (resultException.Result.ResultCode, error);
        }
        catch (Exception ex) {
            this.logger.LogError(ex, "Execute failed with exception. Reload user profile from DB (UserId={UserId}, CommandType={CommandType})",
                this.UserId, command.GetType().Name);

            await this.ReadUserProfileFromDb(createIfNotExist: false);

            throw;
        }
    }

    public ValueTask<Quantum.GameSnapshotLoadout> GetSelectedLoadoutSnapshot() {
        var selectedId = this.userProfile.Loadouts.SelectedLoadout.Value;
        var loadout    = this.userProfile.Loadouts.Get(selectedId);
        var snapshot   = loadout.LoadoutSnapshot.Value;
        return ValueTask.FromResult(snapshot);
    }

    public ValueTask<byte[]> GetSerializedUserProfile() {
        var bytes = this.SerializeSdData();
        return ValueTask.FromResult(bytes);
    }

    public async ValueTask PublishAppServerEvent(IAppServerEvent evt) {
        await this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
            .GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(this.UserId))
            .OnNextAsync(evt);
    }

    public async ValueTask<ServerResult<UserDeleteResponse>> DeleteUserProfile(UserDeleteRequest request) {
        await this.DeleteUserProfileFromDb();
        this.DeactivateOnIdle();
        return new UserDeleteResponse();
    }

    public ValueTask<string> GetNickName() {
        return ValueTask.FromResult(this.userProfile.NickName.Value);
    }

    public ValueTask<int> GetLevel() {
        return ValueTask.FromResult(this.userProfile.Level.Value);
    }

    private Task<ServerCommandResult> ExecuteInternal([NotNull] IUserProfileServerCommand command) {
        ArgumentNullException.ThrowIfNull(command);

        var handler = this.commandHandlerRegistry.GetHandler(command.GetType());
        return handler.Execute(this.commandContext, this.userProfile, command);
    }

    private async ValueTask ReadUserProfileFromDb(bool createIfNotExist) {
        using var context = this.dbContextFactory.CreateDbContext();

        var dbUserProfile = await context.UserProfiles
            .AsNoTracking()
            .Where(profile => profile.Id == this.UserId)
            .Select(profile => new { profile.Data, profile.NickName })
            .FirstOrDefaultAsync();

        if (dbUserProfile == null && createIfNotExist) {
            this.logger.LogInformation("Create DB entry (UserId={UserId})", this.UserId);

            var uniqueNickName = $"Player{this.UserId}";

            var userExists = await context.Users.AnyAsync(u => u.Id == this.UserId);
            if (!userExists) {
                throw new InvalidOperationException($"User not found: {this.UserId}");
            }

            context.UserProfiles.Add(new DbUserProfile {
                Id       = this.UserId,
                Data     = [],
                NickName = uniqueNickName,
            });

            await context.SaveChangesAsync();

            this.userProfile.NickName.Value = uniqueNickName;
        }

        if (dbUserProfile != null) {
            if (dbUserProfile.Data is { Length: > 0 }) {
                this.DeserializeSdData(dbUserProfile.Data);
            }

            this.userProfile.NickName.Value = dbUserProfile.NickName;
        }
    }

    private async ValueTask WriteUserProfileToDb() {
        using var context = this.dbContextFactory.CreateDbContext();

        var bytes = this.SerializeSdData();

        await context.UserProfiles
            .Where(profile => profile.Id == this.UserId)
            .ExecuteUpdateAsync(properties => properties
                .SetProperty(profile => profile.Data, bytes));
    }

    private async ValueTask DeleteUserProfileFromDb() {
        using var context = this.dbContextFactory.CreateDbContext();

        await context.UserProfiles
            .Where(profile => profile.Id == this.UserId)
            .ExecuteDeleteAsync();
    }

    private void DeserializeSdData([NotNull] byte[] bytes) {
        ArgumentNullException.ThrowIfNull(bytes);

        var reader = new MessagePackReader(bytes);
        SdObjectSerializer.Deserialize(this.userProfile, ref reader, MsgPackServerDataOptions);
    }

    private byte[] SerializeSdData() {
        this.userProfileBuffer.Clear();
        SdObjectSerializer.Serialize(this.userProfile, this.userProfileBuffer, MsgPackServerDataOptions);
        return this.userProfileBuffer.WrittenMemory.ToArray();
    }
}