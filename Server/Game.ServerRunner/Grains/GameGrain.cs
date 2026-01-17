namespace Game.ServerRunner.Grains;

using JetBrains.Annotations;
using Multicast;
using Orleans.Concurrency;
using Quantum;
using Shared.DTO;
using Shared.ServerEvents;
using Shared.UserProfile.Commands.Game;
using Shared.UserProfile.Commands.Quests;

public interface IGameGrain : IGrainWithStringKey {
    ValueTask AddPlayer(IUserProfileGrain user);
    ValueTask RemovePlayer(IUserProfileGrain user);

    ValueTask<ServerResult<ReportGameSnapshotResponse>> ReportGameSnapshot(IUserProfileGrain user, ReportGameSnapshotRequest request);

    ValueTask<ServerResult<ReportGameQuestCounterTaskResponse>> ReportCounterTask(
        IUserProfileGrain user, ReportGameQuestCounterTaskRequest request);

    [OneWay] ValueTask NotifyNewGameResult();
}

public static class GameGrainExtensions {
    public static string GetGameId(this IGameGrain grain) => grain.GetPrimaryKeyString();

    public static IGameGrain GetGameGrain(this IGrainFactory grainFactory, string gameId) => grainFactory.GetGrain<IGameGrain>(gameId);
}

public class GameGrain(ILogger<GameGrain> logger) : Grain, IGameGrain {
    private readonly List<Guid> playerIds = new List<Guid>();

    private readonly Dictionary<IUserProfileGrain, GameSnapshot> snapshots = new Dictionary<IUserProfileGrain, GameSnapshot>();

    [CanBeNull] private bool gameResultsApplied;

    public override async Task OnActivateAsync(CancellationToken cancellationToken) {
        await base.OnActivateAsync(cancellationToken);

        this.DelayDeactivation(TimeSpan.FromMinutes(30));
    }

    public async ValueTask AddPlayer(IUserProfileGrain user) {
        if (this.playerIds.Contains(user.GetUserId())) {
            return;
        }

        logger.LogInformation("Player added to the game: GameId={GameId}, UserId={UserId}", this.GetGameId(), user.GetUserId());

        this.playerIds.Add(user.GetUserId());
    }

    public async ValueTask RemovePlayer(IUserProfileGrain user) {
        if (this.playerIds.Contains(user.GetUserId()) == false) {
            return;
        }

        logger.LogInformation("Player removed from the game: GameId={GameId}, UserId={UserId}", this.GetGameId(), user.GetUserId());

        this.playerIds.Remove(user.GetUserId());
    }

    public async ValueTask<ServerResult<ReportGameQuestCounterTaskResponse>> ReportCounterTask(IUserProfileGrain user,
        ReportGameQuestCounterTaskRequest request) {
        //TODO wait for multiple requests for detect cheaters
        if (request.TargetUserIds.Contains(user.GetUserId())) {
            await user.Execute(new UserProfileApplyCounterQuestsCommand {
                Property = request.Property,
                Value    = request.CounterValue,
                Filters  = request.Filters,
            }, UserProfileGrainExecuteOptions.SendUserProfileUpdatedEvent);
        }

        return new ReportGameQuestCounterTaskResponse();
    }

    public async ValueTask<ServerResult<ReportGameSnapshotResponse>> ReportGameSnapshot(IUserProfileGrain user, ReportGameSnapshotRequest request) {
        if (request.GameSnapshot == null) {
            return ServerResult.Error(10, "GameSnapshot is null");
        }

        if (this.playerIds.Contains(user.GetUserId()) == false) {
            logger.LogError("User tried to report game snapshot to not joined game: GameId={GameID}, UserId={UserId}", this.GetGameId(), user.GetUserId());
            return ServerResult.Error(11, "Tried to report game snapshot to not joined game");
        }

        logger.LogInformation("Player reported game snapshot: GameId={GameId}, UserId={UserId}", this.GetGameId(), user.GetUserId());

        if (this.snapshots.TryAdd(user, request.GameSnapshot)) {
            await this.AsReference<IGameGrain>().NotifyNewGameResult();
        }

        return new ReportGameSnapshotResponse {
            ShouldWaitForUserPlayerResults = this.gameResultsApplied == false,
        };
    }

    public async ValueTask NotifyNewGameResult() {
        var currentSnapshotCount  = this.snapshots.Count;
        var requiredSnapshotCount = 1; // (int)Math.Ceiling(this.playerIds.Count / 2f); // TODO wait for multiple results to detect cheaters
        var hasEnoughSnapshots    = currentSnapshotCount >= requiredSnapshotCount;

        await this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
            .GetStream<IGameServerEvent>(OrleansConstants.Streams.Ids.GameServerEventForGame(this.GetGameId()))
            .OnNextAsync(new SnapshotGameServerEvent {
                SnapshotCount         = currentSnapshotCount,
                RequiredSnapshotCount = requiredSnapshotCount,
            });

        if (!hasEnoughSnapshots) {
            return;
        }

        if (this.gameResultsApplied) {
            return;
        }

        this.gameResultsApplied = true;

        // TODO choose best snapshot
        var bestGameSnapshot = this.snapshots.First().Value;

        foreach (var userId in this.playerIds) {
            var user = this.GrainFactory.GetUserProfileGrain(userId);

            await user.Execute(new UserProfileApplyGameResultsCommand {
                GameId      = this.GetGameId(),
                GameSnapshot = bestGameSnapshot,
            }, UserProfileGrainExecuteOptions.SendUserProfileUpdatedEvent);
        }
    }
}