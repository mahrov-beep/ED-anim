namespace Tests.E2E.Party;

using Game.Shared.DTO;
using Multicast;
using Xunit;
public static class PartyApiAssertions {
    public static void ShouldBeInviteSent(this ServerResult<PartyInviteResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EPartyInviteActionStatus.Sent, res.Data.Result);
    }

    public static void ShouldBeAlreadyInvited(this ServerResult<PartyInviteResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EPartyInviteActionStatus.AlreadyInvited, res.Data.Result);
    }

    public static void ShouldBeAlreadyInParty(this ServerResult<PartyInviteResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(EPartyInviteActionStatus.AlreadyInParty, res.Data.Result);
    }

    public static void ShouldBeOk(this ServerResult<PartyAcceptInviteResponse> res) {
        Assert.True(res.DataNotNull());
    }

    public static void ShouldBeOk(this ServerResult<PartyDeclineInviteResponse> res) {
        Assert.True(res.DataNotNull());
    }

    public static void ShouldBeOk(this ServerResult<PartyLeaveResponse> res) {
        Assert.True(res.DataNotNull());
    }

    public static void ShouldBeOk(this ServerResult<PartyKickResponse> res) {
        Assert.True(res.DataNotNull());
    }

    public static void ShouldBeOk(this ServerResult<PartyMakeLeaderResponse> res) {
        Assert.True(res.DataNotNull());
    }

    public static void ShouldBeEmpty(this ServerResult<PartyStatusResponse> res) {
        Assert.True(res.DataNotNull());
        Assert.Equal(Guid.Empty, res.Data.LeaderUserId);
        Assert.NotNull(res.Data.Members);
        Assert.Empty(res.Data.Members);
    }

    public static void ShouldContainMembers(this ServerResult<PartyStatusResponse> res, params Guid[] expectedMembers) {
        Assert.True(res.DataNotNull());
        Assert.NotEqual(Guid.Empty, res.Data.LeaderUserId);
        Assert.NotNull(res.Data.Members);
        foreach (var gid in expectedMembers) {
            Assert.Contains(gid, res.Data.Members);
        }
    }

    public static void ShouldHaveLeader(this ServerResult<PartyStatusResponse> res, Guid expectedLeaderId) {
        Assert.True(res.DataNotNull());
        Assert.Equal(expectedLeaderId, res.Data.LeaderUserId);
    }

    public static void ShouldHaveReadyMembers(this ServerResult<PartyStatusResponse> res, params Guid[] expectedReady) {
        Assert.True(res.DataNotNull());
        Assert.NotNull(res.Data.ReadyMembers);
        foreach (var gid in expectedReady) {
            Assert.Contains(gid, res.Data.ReadyMembers);
        }
    }
}