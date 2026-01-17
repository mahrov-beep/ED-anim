namespace Tests.E2E.Party;

using Game.Shared.DTO;
using Multicast;
using Xunit;
[Collection("e2e-host")]
public class Runner(E2EHost host) {
    private async Task<PartyActor> CreateUserAsync(string deviceId) {
        return await PartyActor.CreateAsync(host, deviceId);
    }

    [Fact]
    public async Task Party_Invite_Sent() {
        PartyActor leader = await CreateUserAsync("party-leader-1");
        PartyActor member = await CreateUserAsync("party-member-1");

        ServerResult<PartyInviteResponse> inv = await leader.InviteAsync(member.Id);
        inv.ShouldBeInviteSent();

        var statusLeader = await leader.StatusAsync();
        statusLeader.ShouldContainMembers(leader.Id);
        var statusMember = await member.StatusAsync();
        statusMember.ShouldBeEmpty();
    }

    [Fact]
    public async Task Party_Invite_Repeat_AlreadyInvited() {
        PartyActor leader = await CreateUserAsync("party-leader-2");
        PartyActor member = await CreateUserAsync("party-member-2");

        _ = await leader.InviteAsync(member.Id);
        var inv2 = await leader.InviteAsync(member.Id);
        inv2.ShouldBeAlreadyInvited();

        var statusLeader = await leader.StatusAsync();
        statusLeader.ShouldContainMembers(leader.Id);
    }

    [Fact]
    public async Task Party_Accept_Then_Decline_And_Leave() {
        PartyActor leader = await CreateUserAsync("party-leader-3");
        PartyActor m1     = await CreateUserAsync("party-m1-3");
        PartyActor m2     = await CreateUserAsync("party-m2-3");

        _ = await leader.InviteAsync(m1.Id);
        _ = await leader.InviteAsync(m2.Id);

        var acc1 = await m1.AcceptAsync(leader.Id);
        acc1.ShouldBeOk();

        var statusAfterAcc1 = await leader.StatusAsync();
        statusAfterAcc1.ShouldContainMembers(leader.Id, m1.Id);

        var dec2 = await m2.DeclineAsync(leader.Id);
        dec2.ShouldBeOk();

        var statusAfterDec2 = await leader.StatusAsync();
        statusAfterDec2.ShouldContainMembers(leader.Id, m1.Id);

        var leave = await m1.LeaveAsync(leader.Id);
        leave.ShouldBeOk();

        var statusAfterLeave = await leader.StatusAsync();
        statusAfterLeave.ShouldContainMembers(leader.Id);
    }

    [Fact]
    public async Task Party_Invite_TargetInAnotherParty_InviteAllowed_And_SwitchOnAccept() {
        PartyActor leader1 = await CreateUserAsync("party-leader-b1");
        PartyActor leader2 = await CreateUserAsync("party-leader-b2");
        PartyActor member  = await CreateUserAsync("party-member-b");

        _ = await leader1.InviteAsync(member.Id);
        _ = await member.AcceptAsync(leader1.Id);

        var status1 = await leader1.StatusAsync();
        status1.ShouldContainMembers(leader1.Id, member.Id);

        var inv2 = await leader2.InviteAsync(member.Id);
        inv2.ShouldBeInviteSent();

        var acc2 = await member.AcceptAsync(leader2.Id);
        acc2.ShouldBeOk();

        var status2 = await leader2.StatusAsync();
        status2.ShouldContainMembers(leader2.Id, member.Id);
        var status1After = await leader1.StatusAsync();
        status1After.ShouldContainMembers(leader1.Id);
    }

    [Fact]
    public async Task Party_LeaderLeaves_PassesLeadership_WhenMembersRemain() {
        PartyActor leader = await CreateUserAsync("party-leader-x1");
        PartyActor m1     = await CreateUserAsync("party-m1-x1");
        PartyActor m2     = await CreateUserAsync("party-m2-x1");

        _ = await leader.InviteAsync(m1.Id);
        _ = await leader.InviteAsync(m2.Id);
        _ = await m1.AcceptAsync(leader.Id);
        _ = await m2.AcceptAsync(leader.Id);

        var leave = await leader.LeaveAsync(leader.Id);
        leave.ShouldBeOk();

        var statusAfterLeaderLeave = await m1.StatusAsync();
        statusAfterLeaderLeave.ShouldContainMembers(m1.Id, m2.Id);
    }

    [Fact]
    public async Task Party_Kick_Member_Removed_From_Party() {
        PartyActor leader = await CreateUserAsync("party-leader-k1");
        PartyActor m1     = await CreateUserAsync("party-m1-k1");

        _ = await leader.InviteAsync(m1.Id);
        _ = await m1.AcceptAsync(leader.Id);

        var before = await leader.StatusAsync();
        before.ShouldContainMembers(leader.Id, m1.Id);

        var kick = await leader.KickAsync(leader.Id, m1.Id);
        kick.ShouldBeOk();

        var statusLeader = await leader.StatusAsync();
        statusLeader.ShouldContainMembers(leader.Id);

        var statusMember = await m1.StatusAsync();
        statusMember.ShouldBeEmpty();
    }

    [Fact]
    public async Task Party_MakeLeader_Transfers_Leadership() {
        PartyActor leader = await CreateUserAsync("party-leader-ml1");
        PartyActor m1     = await CreateUserAsync("party-m1-ml1");

        _ = await leader.InviteAsync(m1.Id);
        _ = await m1.AcceptAsync(leader.Id);

        var before = await leader.StatusAsync();
        before.ShouldContainMembers(leader.Id, m1.Id);

        var make = await leader.MakeLeaderAsync(leader.Id, m1.Id);
        make.ShouldBeOk();

        var statusNewLeader = await m1.StatusAsync();
        statusNewLeader.ShouldContainMembers(m1.Id, leader.Id);
        statusNewLeader.ShouldHaveLeader(m1.Id);

        var statusOldLeader = await leader.StatusAsync();
        statusOldLeader.ShouldContainMembers(m1.Id, leader.Id);
        statusOldLeader.ShouldHaveLeader(m1.Id);
    }

    [Fact]
    public async Task Party_MakeLeader_By_NonLeader_Fails() {
        PartyActor leader = await CreateUserAsync("party-leader-ml2");
        PartyActor m1     = await CreateUserAsync("party-m1-ml2");

        _ = await leader.InviteAsync(m1.Id);
        _ = await m1.AcceptAsync(leader.Id);

        var attempt = await m1.MakeLeaderAsync(leader.Id, m1.Id);
        attempt.AssertDataIsNull();

        var status = await leader.StatusAsync();
        status.ShouldHaveLeader(leader.Id);
    }

    [Fact]
    public async Task Party_Start_By_NonLeader_Fails() {
        PartyActor leader = await CreateUserAsync("party-leader-s1");
        PartyActor m1     = await CreateUserAsync("party-m1-s1");

        _ = await leader.InviteAsync(m1.Id);
        _ = await m1.AcceptAsync(leader.Id);

        var attempt = await m1.StartAsync("factory");
        attempt.AssertDataIsNull();
    }

    [Fact]
    public async Task Party_Start_Blocked_Until_All_Ready() {
        PartyActor leader = await CreateUserAsync("party-leader-s2");
        PartyActor m1     = await CreateUserAsync("party-m1-s2");
        PartyActor m2     = await CreateUserAsync("party-m2-s2");

        _ = await leader.InviteAsync(m1.Id);
        _ = await leader.InviteAsync(m2.Id);
        _ = await m1.AcceptAsync(leader.Id);
        _ = await m2.AcceptAsync(leader.Id);

        var notReadyAttempt = await leader.StartAsync("factory");
        notReadyAttempt.AssertDataIsNull();

        _ = await m1.SetReadyAsync(leader.Id, true);
        var stillBlocked = await leader.StartAsync("factory");
        stillBlocked.AssertDataIsNull();

        _ = await m2.SetReadyAsync(leader.Id, true);

        var ok = await leader.StartAsync("factory");
        Assert.True(ok.DataNotNull());
        Assert.False(string.IsNullOrWhiteSpace(ok.Data.GameId));
    }
}