namespace Game.ServerRunner.Grains;

using Orleans.Concurrency;
using Shared.DTO;
using Shared.ServerEvents;

public interface IUserStatusGrain : IGrainWithGuidKey {
    ValueTask<EUserStatus> GetStatus();
    ValueTask ClientConnected();
    ValueTask ClientDisconnected();
    ValueTask EnterGame();
    ValueTask BackToMenu();
}

public static class UserStatusExtensions {
    public static Guid GetUserId(this IUserStatusGrain grain) => grain.GetPrimaryKey();
    public static IUserStatusGrain GetUserStatusGrain(this IGrainFactory factory, Guid userId) => factory.GetGrain<IUserStatusGrain>(userId);
}

public sealed class UserStatusGrain(
    IGrainFactory grainFactory,
    ILogger<UserStatusGrain> logger)
    : Grain, IUserStatusGrain {

    private int        connectionCount;
    private EUserStatus currentStatus;

    private Guid UserId => this.GetUserId();

    public override Task OnActivateAsync(CancellationToken _) {
        currentStatus   = EUserStatus.Offline;
        connectionCount = 0;
        return Task.CompletedTask;
    }

    public ValueTask<EUserStatus> GetStatus() {
        return ValueTask.FromResult(currentStatus);
    }

    public async ValueTask ClientConnected() {
        var wasOffline = connectionCount == 0;
        connectionCount++;
        if (wasOffline) {
            await SetStatus(EUserStatus.InMenu);
        }
    }

    public async ValueTask ClientDisconnected() {
        if (connectionCount > 0) {
            connectionCount--;
        }
        if (connectionCount == 0 && currentStatus != EUserStatus.Offline) {
            await SetStatus(EUserStatus.Offline);
        }
    }

    public async ValueTask EnterGame() {
        await SetStatus(EUserStatus.InGame);
    }

    public async ValueTask BackToMenu() {
        var target = connectionCount > 0 ? EUserStatus.InMenu : EUserStatus.Offline;
        await SetStatus(target);
    }

    private async ValueTask SetStatus(EUserStatus status) {
        if (currentStatus == status) {
            return;
        }
        currentStatus = status;
        await NotifyFriends(status);
    }

    private async ValueTask NotifyFriends(EUserStatus status) {
        var friendsGrain = grainFactory.GetUserFriendsGrain(UserId);
        var friendIds    = await friendsGrain.GetFriends();

        if (friendIds.Length == 0) {
            return;
        }

        var evt = new FriendStatusChangedAppServerEvent { FriendId = UserId, Status = status };
        var streamProvider = this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS);

        var tasks = new List<Task>(friendIds.Length);
        foreach (var fid in friendIds) {
            var stream = streamProvider.GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(fid));
            tasks.Add(stream.OnNextAsync(evt));
        }
        await Task.WhenAll(tasks);
    }
}