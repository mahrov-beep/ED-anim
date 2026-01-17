namespace Tests.E2E.Friends;

using Game.Shared.DTO;
using Multicast;
using Game.Shared;
using Xunit;
public static class FriendsApiAssertions {
    public static void ShouldContainFriend(this ServerResult<FriendsListResponse> res, Guid other) {
        Assert.True(res.DataNotNull());

        Assert.NotNull(res.Data.Friends);
        Assert.Contains(res.Data.Friends, x => x.Id == other);
    }

    public static void ShouldNotContainFriend(this ServerResult<FriendsListResponse> res, Guid other) {
        Assert.True(res.DataNotNull());

        Assert.NotNull(res.Data.Friends);
        Assert.DoesNotContain(res.Data.Friends, x => x.Id == other);
    }

    public static void ShouldContainIncoming(this ServerResult<IncomingRequestsResponse> res, Guid other) {
        Assert.True(res.DataNotNull());

        Assert.NotNull(res.Data.IncomingRequests);
        Assert.Contains(res.Data.IncomingRequests, x => x.Id == other);
    }

    public static void ShouldNotContainIncoming(this ServerResult<IncomingRequestsResponse> res, Guid other) {
        Assert.True(res.DataNotNull());

        Assert.NotNull(res.Data.IncomingRequests);
        Assert.DoesNotContain(res.Data.IncomingRequests, x => x.Id == other);
    }

    public static void ShouldBePendingCreated(this ServerResult<FriendAddResponse> res) {
        Assert.True(res.DataNotNull());

        Assert.Equal(EFriendActionStatus.PendingCreated, res.Data.Result);
        Assert.True(res.Data.Created);
    }

    public static void ShouldBeAlreadyPendingOutgoing(this ServerResult<FriendAddResponse> res) {
        Assert.True(res.DataNotNull());

        Assert.Equal(EFriendActionStatus.AlreadyPendingOutgoing, res.Data.Result);
    }

    public static void ShouldBeAlreadyFriends(this ServerResult<FriendAcceptResponse> res) {
        Assert.True(res.DataNotNull());

        Assert.Equal(EFriendActionStatus.AlreadyFriends, res.Data.Result);
    }

    public static void ShouldBeNotFound(this ServerResult<FriendAcceptResponse> res) {
        Assert.True(res.DataNotNull());

        Assert.Equal(EFriendActionStatus.NotFound, res.Data.Result);
    }

    public static void ShouldBeRemoved(this ServerResult<FriendDeclineResponse> res) {
        Assert.True(res.DataNotNull());

        Assert.Equal(EFriendActionStatus.Removed, res.Data.Result);
    }

    public static void ShouldBeRemoved(this ServerResult<FriendRemoveResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EFriendActionStatus.Removed, res.Data.Result);
    }

    public static void ShouldBeAlreadyFriendsNotCreated(this ServerResult<FriendAddResponse> res) {
        Assert.True(res.DataNotNull());

        Assert.Equal(EFriendActionStatus.AlreadyFriends, res.Data.Result);
        Assert.False(res.Data.Created);
    }
}