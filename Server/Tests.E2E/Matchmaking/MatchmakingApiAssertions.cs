namespace Tests.E2E.Matchmaking;

using Game.Shared.DTO;
using Multicast;
using Xunit;
public static class MatchmakingApiAssertions {
    public static void ShouldBeEnqueued(this ServerResult<MatchmakingJoinResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EMatchmakingJoinStatus.Enqueued, res.Data.Result);
    }

    public static void ShouldBeAlreadyQueued(this ServerResult<MatchmakingJoinResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EMatchmakingJoinStatus.AlreadyQueued, res.Data.Result);
    }

    public static void ShouldBeOk(this ServerResult<MatchmakingCancelResponse> res) {
        Assert.True(res.DataNotNull());
    }

    public static void ShouldBeIdle(this ServerResult<MatchmakingStatusResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EMatchmakingQueueStatus.Idle, res.Data.Status);
    }

    public static void ShouldBeQueuedFor(this ServerResult<MatchmakingStatusResponse> res, string gameModeKey) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EMatchmakingQueueStatus.Queued, res.Data.Status);
        Assert.Equal(gameModeKey, res.Data.GameModeKey);
    }

    public static void ShouldBeMatchedWithContract(this ServerResult<MatchmakingStatusResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EMatchmakingQueueStatus.Matched, res.Data.Status);
        Assert.NotNull(res.Data.Join);
        Assert.False(string.IsNullOrWhiteSpace(res.Data.Join.Region));
        Assert.NotNull(res.Data.Join.ExpectedUsers);
        Assert.NotEmpty(res.Data.Join.ExpectedUsers);
        Assert.NotNull(res.Data.Join.RoomPropsMinimal);
        Assert.False(string.IsNullOrWhiteSpace(res.Data.Join.Ticket));
    }
}