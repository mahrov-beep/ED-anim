namespace Game.ServerRunner.Grains;

using Db;
using Db.Model;
using Microsoft.EntityFrameworkCore;
using Shared.DTO;
using Orleans;
using Shared.ServerEvents;
using Game.ServerRunner;
[GenerateSerializer]
public sealed class FriendActionResult {
    [Id(0)] public EFriendActionStatus Status  { get; init; }
    [Id(1)] public Guid                SelfId  { get; init; }
    [Id(2)] public Guid                OtherId { get; init; }
}
public interface IUserFriendsGrain : IGrainWithGuidKey {
    // A -> B: послать запрос (или "слить" во френдов при взаимном)
    ValueTask<FriendActionResult> RequestFriendship(Guid otherId);

    // B -> A: принять входящий запрос
    ValueTask<FriendActionResult> AcceptFriendship(Guid otherId);

    // B -> A: отклонить входящий запрос
    ValueTask<FriendActionResult> RejectFriendshipRequest(Guid otherId);

    // A <-> B: удалить из друзей
    ValueTask<FriendActionResult> RemoveFriend(Guid otherId);

    ValueTask NotifyIncoming(Guid requesterId);
    ValueTask NotifyAccepted(Guid friendId);
    ValueTask NotifyRemoved(Guid friendId);

    ValueTask<Guid[]> GetFriends();
    ValueTask<Guid[]> GetIncomingRequests();

    ValueTask<Guid[]> GetOnlineFriends();

    ValueTask ProcessIncomingBulk(EFriendBulkAction action);
}
public static class UserFriendsExtension {
    public static Guid GetUserId(this IUserFriendsGrain grain) =>
        grain.GetPrimaryKey();

    public static IUserFriendsGrain GetUserFriendsGrain(this IGrainFactory factory, Guid userId) =>
        factory.GetGrain<IUserFriendsGrain>(userId);
}
public sealed class UserFriendsGrain(
    IDbContextFactory<GameDbContext> dbContext,
    IGrainFactory grainFactory,
    ILogger<UserFriendsGrain> logger)
    : Grain, IUserFriendsGrain {

    private readonly HashSet<Guid> friends  = [];
    private readonly HashSet<Guid> incoming = [];
    private readonly HashSet<Guid> outgoing = [];

    private Guid UserId => this.GetUserId();

    public override async Task OnActivateAsync(CancellationToken _) {
        var me = UserId;

        await using GameDbContext db = await dbContext.CreateDbContextAsync();

        var acceptedFriends = await db.Friendship
            .Where(x => x.Status == EFriendStatus.Accepted && (x.UserA == me || x.UserB == me))
            .Select(x => x.UserA == me ? x.UserB : x.UserA)
            .ToListAsync(_);

        friends.Clear();
        foreach (var f in acceptedFriends) {
            friends.Add(f);
        }

        var incomingRequests = await db.Friendship
            .Where(x => x.Status == EFriendStatus.Pending && x.AddresseeId == me)
            .Select(x => x.RequesterId)
            .ToListAsync(_);

        incoming.Clear();
        foreach (var f in incomingRequests) {
            incoming.Add(f);
        }

        var outgoingRequests = await db.Friendship
            .Where(x => x.Status == EFriendStatus.Pending && x.RequesterId == me)
            .Select(x => x.AddresseeId)
            .ToListAsync(_);

        outgoing.Clear();
        foreach (var f in outgoingRequests) {
            outgoing.Add(f);
        }
    }

    public async ValueTask<FriendActionResult> RequestFriendship(Guid otherId) {
        var me = UserId;

        if (otherId == Guid.Empty) {
            return new FriendActionResult { Status = EFriendActionStatus.Error, SelfId = me, OtherId = otherId };
        }

        if (otherId == me) {
            return new FriendActionResult { Status = EFriendActionStatus.SelfNotAllowed, SelfId = me, OtherId = otherId };
        }

        var now = DateTime.UtcNow;
        var (ua, ub) = PostgressHelper.CompareGuidAsPostgres(me, otherId) < 0 ? (me, otherId) : (otherId, me);
        var otherGrain = grainFactory.GetUserFriendsGrain(otherId);

        await using GameDbContext db = await dbContext.CreateDbContextAsync();

        for (int attempt = 0; attempt < 2; attempt++) {
            var row = await db.Friendship.SingleOrDefaultAsync(x => x.UserA == ua && x.UserB == ub);

            if (row is null) {
                var entity = new DbFriendship {
                    RequesterId = me,
                    AddresseeId = otherId,
                    Status      = EFriendStatus.Pending,
                    Created     = now,
                    Updated     = now,
                };
                db.Friendship.Add(entity);
                try {
                    await db.SaveChangesAsync();

                    outgoing.Add(otherId);
                    await otherGrain.NotifyIncoming(me);

                    return new FriendActionResult { Status = EFriendActionStatus.PendingCreated, SelfId = me, OtherId = otherId };
                }
                catch (DbUpdateException) {
                    db.ChangeTracker.Clear();
                    continue;
                }
            }

            if (row.Status == EFriendStatus.Accepted) {
                incoming.Remove(otherId);
                outgoing.Remove(otherId);
                var added = friends.Add(otherId);
                if (added) {
                    await PublishAppEvent(me, new FriendAddedAppServerEvent { FriendId = otherId });
                }
                return new FriendActionResult { Status = EFriendActionStatus.AlreadyFriends, SelfId = me, OtherId = otherId };
            }

            // Pending exists
            if (row.RequesterId == me && row.AddresseeId == otherId) {
                outgoing.Add(otherId);
                return new FriendActionResult { Status = EFriendActionStatus.AlreadyPendingOutgoing, SelfId = me, OtherId = otherId };
            }

            if (row.RequesterId == otherId && row.AddresseeId == me) {
                row.Status     = EFriendStatus.Accepted;
                row.AcceptedAt = now;
                row.Updated    = now;
                await db.SaveChangesAsync();

                incoming.Remove(otherId);
                outgoing.Remove(otherId);
                friends.Add(otherId);

                await otherGrain.NotifyAccepted(me);
                await PublishAppEvent(me, new FriendAddedAppServerEvent { FriendId = otherId });

                return new FriendActionResult { Status = EFriendActionStatus.AlreadyFriends, SelfId = me, OtherId = otherId };
            }

            logger.LogError("Unexpected friendship row state for users {UserA} and {UserB}", ua, ub);
            return new FriendActionResult { Status = EFriendActionStatus.Error, SelfId = me, OtherId = otherId };
        }

        var final = await db.Friendship.SingleOrDefaultAsync(x => x.UserA == ua && x.UserB == ub);
        if (final is null) {
            return new FriendActionResult { Status = EFriendActionStatus.Error, SelfId = me, OtherId = otherId };
        }
        if (final.Status == EFriendStatus.Accepted) {
            incoming.Remove(otherId);
            outgoing.Remove(otherId);
            var added = friends.Add(otherId);
            if (added) {
                await PublishAppEvent(me, new FriendAddedAppServerEvent { FriendId = otherId });
            }
            return new FriendActionResult { Status = EFriendActionStatus.AlreadyFriends, SelfId = me, OtherId = otherId };
        }
        if (final.RequesterId == me) {
            return new FriendActionResult { Status = EFriendActionStatus.AlreadyPendingOutgoing, SelfId = me, OtherId = otherId };
        }
        incoming.Add(otherId);
        return new FriendActionResult { Status = EFriendActionStatus.AlreadyPendingIncoming, SelfId = me, OtherId = otherId };
    }

    public async ValueTask<FriendActionResult> AcceptFriendship(Guid otherId) {
        var me = UserId;

        if (otherId == Guid.Empty) {
            return new FriendActionResult { Status = EFriendActionStatus.Error, SelfId = me, OtherId = otherId };
        }

        if (otherId == me) {
            return new FriendActionResult { Status = EFriendActionStatus.SelfNotAllowed, SelfId = me, OtherId = otherId };
        }

        var now = DateTime.UtcNow;
        var (ua, ub) = PostgressHelper.CompareGuidAsPostgres(me, otherId) < 0 ? (me, otherId) : (otherId, me);

        await using GameDbContext db  = await dbContext.CreateDbContextAsync();
        var                       row = await db.Friendship.SingleOrDefaultAsync(x => x.UserA == ua && x.UserB == ub);
        if (row is null) {
            return new FriendActionResult { Status = EFriendActionStatus.NotFound, SelfId = me, OtherId = otherId };
        }

        if (row.Status == EFriendStatus.Accepted) {
            incoming.Remove(otherId);
            outgoing.Remove(otherId);
            var added = friends.Add(otherId);

            if (added) {
                await PublishAppEvent(me, new FriendAddedAppServerEvent { FriendId = otherId });
            }

            return new FriendActionResult { Status = EFriendActionStatus.AlreadyFriends, SelfId = me, OtherId = otherId };
        }

        if (row.Status == EFriendStatus.Pending && row.RequesterId == otherId && row.AddresseeId == me) {

            row.Status     = EFriendStatus.Accepted;
            row.AcceptedAt = now;
            row.Updated    = now;

            await db.SaveChangesAsync();

            incoming.Remove(otherId);
            outgoing.Remove(otherId);
            friends.Add(otherId);

            var other = grainFactory.GetUserFriendsGrain(otherId);
            await other.NotifyAccepted(me);

            await PublishAppEvent(me, new FriendAddedAppServerEvent { FriendId = otherId });

            return new FriendActionResult { Status = EFriendActionStatus.AlreadyFriends, SelfId = me, OtherId = otherId };
        }

        return new FriendActionResult { Status = EFriendActionStatus.NotFound, SelfId = me, OtherId = otherId };
    }

    public async ValueTask<FriendActionResult> RejectFriendshipRequest(Guid otherId) {
        var me = UserId;

        if (otherId == Guid.Empty) {
            return new FriendActionResult { Status = EFriendActionStatus.Error, SelfId = me, OtherId = otherId };
        }

        if (otherId == me) {
            return new FriendActionResult { Status = EFriendActionStatus.SelfNotAllowed, SelfId = me, OtherId = otherId };
        }

        var (ua, ub) = PostgressHelper.CompareGuidAsPostgres(me, otherId) < 0 ? (me, otherId) : (otherId, me);

        await using GameDbContext db = await dbContext.CreateDbContextAsync();

        var row = await db.Friendship.SingleOrDefaultAsync(x => x.UserA == ua && x.UserB == ub);

        if (row is null) {
            return new FriendActionResult { Status = EFriendActionStatus.NotFound, SelfId = me, OtherId = otherId };
        }

        if (row.Status == EFriendStatus.Pending && row.AddresseeId == me) {
            db.Friendship.Remove(row);
            await db.SaveChangesAsync();

            incoming.Remove(otherId);

            var other = grainFactory.GetUserFriendsGrain(otherId);
            await other.NotifyRemoved(me);

            return new FriendActionResult { Status = EFriendActionStatus.Removed, SelfId = me, OtherId = otherId };
        }

        if (row.Status == EFriendStatus.Accepted) {
            return new FriendActionResult { Status = EFriendActionStatus.AlreadyFriends, SelfId = me, OtherId = otherId };
        }

        return new FriendActionResult { Status = EFriendActionStatus.NotFound, SelfId = me, OtherId = otherId };
    }

    public async ValueTask<FriendActionResult> RemoveFriend(Guid otherId) {
        var me = UserId;

        if (otherId == Guid.Empty) {
            return new FriendActionResult { Status = EFriendActionStatus.Error, SelfId = me, OtherId = otherId };
        }

        if (otherId == me) {
            return new FriendActionResult { Status = EFriendActionStatus.SelfNotAllowed, SelfId = me, OtherId = otherId };
        }

        var (ua, ub) = PostgressHelper.CompareGuidAsPostgres(me, otherId) < 0 ? (me, otherId) : (otherId, me);

        await using GameDbContext db = await dbContext.CreateDbContextAsync();

        var row = await db.Friendship.SingleOrDefaultAsync(x => x.UserA == ua && x.UserB == ub);

        if (row is null || row.Status != EFriendStatus.Accepted) {
            return new FriendActionResult { Status = EFriendActionStatus.NotFound, SelfId = me, OtherId = otherId };
        }

        db.Friendship.Remove(row);
        await db.SaveChangesAsync();

        var wasFriend = friends.Remove(otherId);
        if (wasFriend) {
            await PublishAppEvent(me, new FriendRemovedAppServerEvent { FriendId = otherId });
        }

        var other = grainFactory.GetUserFriendsGrain(otherId);
        await other.NotifyRemoved(me);

        return new FriendActionResult { Status = EFriendActionStatus.Removed, SelfId = me, OtherId = otherId };
    }

    public async ValueTask NotifyIncoming(Guid requesterId) {
        var me = UserId;

        if (requesterId == Guid.Empty) {
            return;
        }

        if (requesterId == me) {
            return;
        }

        if (friends.Contains(requesterId)) {
            return;
        }

        if (outgoing.Contains(requesterId)) {
            var accept = await AcceptFriendship(requesterId);
            if (accept.Status == EFriendActionStatus.AlreadyFriends) {
                return;
            }

            if (incoming.Add(requesterId)) {
                await PublishAppEvent(me, new FriendRequestIncomingAppServerEvent { RequesterId = requesterId });
            }

            return;
        }

        if (incoming.Add(requesterId)) {
            await PublishAppEvent(me, new FriendRequestIncomingAppServerEvent { RequesterId = requesterId });
        }
    }

    public async ValueTask NotifyAccepted(Guid friendId) {
        var me = UserId;
        if (friendId == Guid.Empty) {
            return;
        }

        if (friendId == me) {
            return;
        }

        incoming.Remove(friendId);
        outgoing.Remove(friendId);
        var added = friends.Add(friendId);
        if (added) {
            await PublishAppEvent(me, new FriendAddedAppServerEvent { FriendId = friendId });
        }
    }

    public async ValueTask NotifyRemoved(Guid friendId) {
        var me = UserId;

        if (friendId == Guid.Empty) {
            return;
        }

        if (friendId == me) {
            return;
        }

        incoming.Remove(friendId);
        outgoing.Remove(friendId);
        friends.Remove(friendId);

        await PublishAppEvent(me, new FriendRemovedAppServerEvent { FriendId = friendId });
    }

    public async ValueTask<Guid[]> GetFriends() {
        return friends.ToArray();
    }

    public async ValueTask<Guid[]> GetIncomingRequests() {
        return incoming.ToArray();
    }

    public async ValueTask<Guid[]> GetOnlineFriends() {
        if (friends.Count == 0) {
            return Array.Empty<Guid>();
        }

        var results = new List<Guid>(friends.Count);
        foreach (var fid in friends) {
            var statusGrain = grainFactory.GetGrain<IUserStatusGrain>(fid);
            var status      = await statusGrain.GetStatus();
            if (status == EUserStatus.InMenu) {
                results.Add(fid);
            }
        }
        return results.ToArray();
    }

    public async ValueTask ProcessIncomingBulk(EFriendBulkAction action) {
        var targets = incoming.ToArray();

        foreach (var id in targets) {
            _ = action switch {
                EFriendBulkAction.Accept => await AcceptFriendship(id),
                EFriendBulkAction.Decline => await RejectFriendshipRequest(id),
            };
        }
    }

    private async ValueTask PublishAppEvent(Guid targetUserId, IAppServerEvent evt) {
        await this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
            .GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(targetUserId))
            .OnNextAsync(evt);
    }
}