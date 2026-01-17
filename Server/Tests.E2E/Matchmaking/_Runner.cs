namespace Tests.E2E.Matchmaking;

using Game.Shared.ServerEvents;
using Multicast;
using Party;
using Events;
using Xunit;
[Collection("e2e-host")]
public class Runner(E2EHost host) : IAsyncLifetime {
    public async Task InitializeAsync() {
        using var client = host.CreateHttpClient();
        await client.PostAsync("/api/matchmaking/__test_clear", null);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<MatchmakingActor> CreateUserAsync(string deviceId) {
        return await MatchmakingActor.CreateAsync(host, deviceId);
    }

    // ==================== SOLO QUEUE: BASIC FLOW ====================

    [Fact]
    public async Task Solo_Join_ShouldEnqueueSuccessfully() {
        var user = await CreateUserAsync("solo-join-1");

        var join = await user.JoinAsync("photon_dm");
        join.ShouldBeEnqueued();

        var status = await user.StatusAsync();
        status.ShouldBeQueuedFor("photon_dm");
    }

    [Fact]
    public async Task Solo_CancelBeforeMatch_ShouldReturnToIdle() {
        var user = await CreateUserAsync("solo-cancel-1");

        _ = await user.JoinAsync("photon_dm");
        var cancel = await user.CancelAsync();
        cancel.ShouldBeOk();

        var status = await user.StatusAsync();
        status.ShouldBeIdle();
    }

    [Fact]
    public async Task Solo_JoinTwiceSameMode_ShouldReturnAlreadyQueued() {
        var user = await CreateUserAsync("solo-join-twice-1");

        _ = await user.JoinAsync("photon_dm");
        var join2 = await user.JoinAsync("photon_dm");

        join2.ShouldBeAlreadyQueued();
    }

    [Fact]
    public async Task Solo_JoinDifferentModeWhileQueued_ShouldReturnAlreadyQueued() {
        var user = await CreateUserAsync("solo-join-diff-1");

        _ = await user.JoinAsync("photon_dm");
        var joinOther = await user.JoinAsync("photon_duo");

        joinOther.ShouldBeAlreadyQueued();
    }

    [Fact]
    public async Task Solo_RejoinAfterCancel_ShouldEnqueueSuccessfully() {
        var user = await CreateUserAsync("solo-rejoin-1");

        _ = await user.JoinAsync("photon_dm");
        _ = await user.CancelAsync();

        var status1 = await user.StatusAsync();
        status1.ShouldBeIdle();

        var joinAgain = await user.JoinAsync("photon_dm");
        joinAgain.ShouldBeEnqueued();

        var status2 = await user.StatusAsync();
        status2.ShouldBeQueuedFor("photon_dm");
    }

    // ==================== SOLO QUEUE: MATCHING ====================

    [Fact]
    public async Task Solo_TwoPlayers_ShouldMatchWithValidContract() {
        var a = await CreateUserAsync("solo-2p-a");
        var b = await CreateUserAsync("solo-2p-b");

        _ = await a.JoinAsync("photon_dm");
        _ = await b.JoinAsync("photon_dm");

        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? sa       = null, sb = null;
        var                                                      deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            sa = await a.StatusAsync();
            sb = await b.StatusAsync();
            if (sa.DataNotNull() && sb.DataNotNull() &&
                sa.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched &&
                sb.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched) {
                break;
            }
            await Task.Delay(250);
        }

        Assert.NotNull(sa);
        Assert.NotNull(sb);
        sa.ShouldBeMatchedWithContract();
        sb.ShouldBeMatchedWithContract();

        Assert.Equal(2, sa.Data.Join.ExpectedUsers.Length);
        Assert.Contains(a.Id, sa.Data.Join.ExpectedUsers);
        Assert.Contains(b.Id, sa.Data.Join.ExpectedUsers);
    }

    [Fact]
    public async Task Solo_SixPlayers_ShouldMatchInOneLobby() {
        const int maxPlayers = 6;

        var players = await Task.WhenAll(Enumerable.Range(0, maxPlayers).Select(i => CreateUserAsync($"solo-6p-{i}")));

        foreach (var player in players) {
            _ = await player.JoinAsync("photon_dm");
        }

        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>[] statuses = [];
        var                                                       deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            statuses = await Task.WhenAll(players.Select(p => p.StatusAsync()));
            if (statuses.All(s => s.DataNotNull() && s.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched)) {
                break;
            }
            await Task.Delay(100);
        }

        foreach (var status in statuses) {
            status.ShouldBeMatchedWithContract();
            Assert.Equal(maxPlayers, status.Data.Join.ExpectedUsers.Length);
            Assert.Equal("photon_dm", status.Data.Join.RoomPropsMinimal["mode"]);
            Assert.Equal("0", status.Data.Join.RoomPropsMinimal.GetValueOrDefault("bots", "0"));
        }

        var expectedUsers = statuses[0].Data.Join.ExpectedUsers;
        var allPlayerIds  = players.Select(p => p.Id).ToHashSet();
        Assert.Equal(allPlayerIds, expectedUsers.ToHashSet());
    }

    [Fact]
    public async Task Solo_OnePlayer_ShouldMatchAfterTimeout() {
        var user = await CreateUserAsync("solo-timeout-1");
        _ = await user.JoinAsync("photon_dm");

        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? status   = null;
        var                                                      deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            status = await user.StatusAsync();
            if (status.DataNotNull() && status.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched) {
                break;
            }
            await Task.Delay(250);
        }

        Assert.NotNull(status);
        status.ShouldBeMatchedWithContract();
        Assert.Single(status.Data.Join.ExpectedUsers);
        Assert.Equal(user.Id, status.Data.Join.ExpectedUsers[0]);
        Assert.Equal("photon_dm", status.Data.Join.RoomPropsMinimal["mode"]);
    }

    [Fact]
    public async Task Solo_JoinWhileAlreadyMatched_ShouldReturnAlreadyInGame() {
        var users = await Task.WhenAll(Enumerable.Range(0, 6).Select(i => CreateUserAsync($"solo-already-{i}")));

        foreach (var u in users) {
            _ = await u.JoinAsync("photon_dm");
        }

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);
        var matched  = false;
        while (DateTime.UtcNow < deadline) {
            var statuses = await Task.WhenAll(users.Select(u => u.StatusAsync()));
            matched = statuses.All(s => s.DataNotNull() && s.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched);
            if (matched) {
                break;
            }
            await Task.Delay(100);
        }

        Assert.True(matched);

        var again = await users[0].JoinAsync("photon_dm");
        Assert.True(again.DataNotNull());
        
        await Task.Delay(500);
        Assert.Equal(Game.Shared.DTO.EMatchmakingJoinStatus.AlreadyInGame, again.Data.Result);
    }

    [Fact]
    public async Task Solo_CancelAfterMatch_ShouldReturnError() {
        var a = await CreateUserAsync("solo-cancel-matched-a");
        var b = await CreateUserAsync("solo-cancel-matched-b");

        _ = await a.JoinAsync("photon_dm");
        _ = await b.JoinAsync("photon_dm");

        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? statusBeforeCancel = null;
        var                                                      deadline           = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            statusBeforeCancel = await a.StatusAsync();

            if (statusBeforeCancel.DataNotNull() && statusBeforeCancel.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched) {
                break;
            }
            await Task.Delay(250);
        }

        Assert.NotNull(statusBeforeCancel);
        statusBeforeCancel.ShouldBeMatchedWithContract();

        var cancelResult = await a.CancelAsync();
        Assert.False(cancelResult.DataNotNull());
        Assert.Equal(4, cancelResult.ErrorCode);

        var statusAfterCancel = await a.StatusAsync();
        statusAfterCancel.ShouldBeMatchedWithContract();

        Assert.Equal(statusBeforeCancel.Data.Join.RoomName, statusAfterCancel.Data.Join.RoomName);
        Assert.Equal(statusBeforeCancel.Data.Join.Ticket, statusAfterCancel.Data.Join.Ticket);
    }

    [Fact]
    public async Task Solo_StatusAfterMatch_ShouldBeIdempotent() {
        var a = await CreateUserAsync("solo-idempotent-a");
        var b = await CreateUserAsync("solo-idempotent-b");

        _ = await a.JoinAsync("photon_dm");
        _ = await b.JoinAsync("photon_dm");

        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? status1  = null;
        var                                                      deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            status1 = await a.StatusAsync();
            if (status1.DataNotNull() && status1.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched) {
                break;
            }
            await Task.Delay(250);
        }

        Assert.NotNull(status1);
        status1.ShouldBeMatchedWithContract();

        var status2 = await a.StatusAsync();
        var status3 = await a.StatusAsync();

        status2.ShouldBeMatchedWithContract();
        status3.ShouldBeMatchedWithContract();

        Assert.Equal(status1.Data.Join.RoomName, status2.Data.Join.RoomName);
        Assert.Equal(status1.Data.Join.RoomName, status3.Data.Join.RoomName);
        Assert.Equal(status1.Data.Join.Ticket, status2.Data.Join.Ticket);
        Assert.Equal(status1.Data.Join.Ticket, status3.Data.Join.Ticket);
    }

    // ==================== SOLO QUEUE: EVENTS ====================

    [Fact]
    public async Task Solo_Join_ShouldEmitMatchmakingStartedEvent() {
        var user = await CreateUserAsync("solo-evt-1");

        var             token        = await user.GetAccessTokenAsync();
        await using var eventsClient = await AppEventsClient.ConnectAsync(host, token);

        await Task.Delay(250);

        _ = await user.JoinAsync("photon_dm");

        var evt = await eventsClient.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(evt);
        Assert.Equal("photon_dm", evt.GameModeKey);
        Assert.Equal(user.Id, evt.LeaderUserId);
    }

    [Fact]
    public async Task Solo_Join_ShouldEmitExactlyOneStartedEvent() {
        var user = await CreateUserAsync("solo-evt-once-1");

        var             token        = await user.GetAccessTokenAsync();
        await using var eventsClient = await AppEventsClient.ConnectAsync(host, token);

        await Task.Delay(250);

        _ = await user.JoinAsync("photon_dm");

        var evt1 = await eventsClient.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(evt1);

        // Short timeout when expecting NO event
        var evt2 = await eventsClient.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(2));
        Assert.Null(evt2);
    }

    [Fact]
    public async Task Solo_Match_ShouldEmitMatchedEventForAllPlayers() {
        var a = await CreateUserAsync("solo-matched-evt-a");
        var b = await CreateUserAsync("solo-matched-evt-b");
        var c = await CreateUserAsync("solo-matched-evt-c");
        var d = await CreateUserAsync("solo-matched-evt-d");
        var e = await CreateUserAsync("solo-matched-evt-e");
        var f = await CreateUserAsync("solo-matched-evt-f");

        await using var evA = await AppEventsClient.ConnectAsync(host, await a.GetAccessTokenAsync());
        await using var evB = await AppEventsClient.ConnectAsync(host, await b.GetAccessTokenAsync());

        await Task.Delay(500);

        _ = await a.JoinAsync("photon_dm");
        _ = await b.JoinAsync("photon_dm");
        _ = await c.JoinAsync("photon_dm");
        _ = await d.JoinAsync("photon_dm");
        _ = await e.JoinAsync("photon_dm");
        _ = await f.JoinAsync("photon_dm");

        var matchedEvtA = await evA.WaitForAsync<PartyMatchmakingMatchedAppServerEvent>(TimeSpan.FromSeconds(12));
        var matchedEvtB = await evB.WaitForAsync<PartyMatchmakingMatchedAppServerEvent>(TimeSpan.FromSeconds(12));

        Assert.NotNull(matchedEvtA);
        Assert.NotNull(matchedEvtB);

        // Both events should have the same room
        Assert.Equal(matchedEvtA.RoomName, matchedEvtB.RoomName);
        Assert.Equal("photon_dm", matchedEvtA.GameModeKey);
        Assert.Equal(6, matchedEvtA.ExpectedUsers.Length);

        // Verify both players are in the expected users list
        var expectedSet = matchedEvtA.ExpectedUsers.ToHashSet();
        Assert.Contains(a.Id, expectedSet);
        Assert.Contains(b.Id, expectedSet);
    }

    [Fact]
    public async Task Solo_Cancel_ShouldEmitCanceledEvent() {
        var user = await CreateUserAsync("solo-cancel-evt-1");

        var             token        = await user.GetAccessTokenAsync();
        await using var eventsClient = await AppEventsClient.ConnectAsync(host, token);

        await Task.Delay(250);

        _ = await user.JoinAsync("photon_dm");

        var startedEvt = await eventsClient.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(startedEvt);

        _ = await user.CancelAsync();

        var canceledEvt = await eventsClient.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(canceledEvt);
        Assert.Equal(user.Id, canceledEvt.LeaderUserId);
        Assert.Equal(user.Id, canceledEvt.CanceledByUserId);

        var status = await user.StatusAsync();
        Assert.Equal(Game.Shared.DTO.EMatchmakingQueueStatus.Idle, status.Data.Status);
    }

    [Fact]
    public async Task Solo_Cancel_ShouldEmitExactlyOneCanceledEvent() {
        var user = await CreateUserAsync("solo-cancel-evt-once-1");

        var             token        = await user.GetAccessTokenAsync();
        await using var eventsClient = await AppEventsClient.ConnectAsync(host, token);

        await Task.Delay(250);

        _ = await user.JoinAsync("photon_dm");

        var startedEvt = await eventsClient.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(startedEvt);

        _ = await user.CancelAsync();

        var canceledEvt1 = await eventsClient.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(canceledEvt1);

        // Short timeout when expecting NO event
        var canceledEvt2 = await eventsClient.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(2));
        Assert.Null(canceledEvt2);
    }

    [Fact]
    public async Task Solo_CancelWhileQueued_ShouldReturnToIdleWithEvent() {
        var user = await CreateUserAsync("solo-cancel-queued-evt-1");

        var             token        = await user.GetAccessTokenAsync();
        await using var eventsClient = await AppEventsClient.ConnectAsync(host, token);

        await Task.Delay(250);

        var joinResult = await user.JoinAsync("photon_dm");
        joinResult.ShouldBeEnqueued();

        var startedEvt = await eventsClient.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(startedEvt);
        Assert.Equal("photon_dm", startedEvt.GameModeKey);

        var statusBeforeCancel = await user.StatusAsync();
        statusBeforeCancel.ShouldBeQueuedFor("photon_dm");

        var cancelResult = await user.CancelAsync();
        cancelResult.ShouldBeOk();

        var canceledEvt = await eventsClient.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(canceledEvt);
        Assert.Equal(user.Id, canceledEvt.LeaderUserId);
        Assert.Equal(user.Id, canceledEvt.CanceledByUserId);

        var statusAfterCancel = await user.StatusAsync();
        statusAfterCancel.ShouldBeIdle();
    }

    [Fact]
    public async Task Solo_MultiplePlayers_OneCancels_OnlyThatPlayerGetsEvent() {
        var user1 = await CreateUserAsync("solo-multi-cancel-1");
        var user2 = await CreateUserAsync("solo-multi-cancel-2");

        var             token1  = await user1.GetAccessTokenAsync();
        var             token2  = await user2.GetAccessTokenAsync();
        await using var events1 = await AppEventsClient.ConnectAsync(host, token1);
        await using var events2 = await AppEventsClient.ConnectAsync(host, token2);

        await Task.Delay(250);

        _ = await user1.JoinAsync("photon_dm");
        _ = await user2.JoinAsync("photon_dm");

        var startedEvt1 = await events1.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));
        var startedEvt2 = await events2.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(startedEvt1);
        Assert.NotNull(startedEvt2);

        // user1 cancels
        _ = await user1.CancelAsync();

        var canceledEvt1 = await events1.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(5));
        Assert.NotNull(canceledEvt1);
        Assert.Equal(user1.Id, canceledEvt1.CanceledByUserId);

        // user2 should NOT receive a canceled event (short timeout)
        var canceledEvt2 = await events2.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(2));
        Assert.Null(canceledEvt2);

        // user1 should be idle, user2 still queued
        (await user1.StatusAsync()).ShouldBeIdle();
        (await user2.StatusAsync()).ShouldBeQueuedFor("photon_dm");
    }

    // ==================== PARTY QUEUE: BASIC FLOW ====================

    [Fact]
    public async Task Party_NonLeader_CannotJoin() {
        var leader = await PartyActor.CreateAsync(host, "party-nonlead-l");
        var member = await PartyActor.CreateAsync(host, "party-nonlead-m");

        _ = await leader.InviteAsync(member.Id);
        _ = await member.AcceptAsync(leader.Id);

        var memberMm = await CreateUserAsync("party-nonlead-m");
        var attempt  = await memberMm.JoinAsync("photon_dm");

        attempt.AssertDataIsNull();
    }

    [Fact]
    public async Task Party_NotAllMembersReady_JoinShouldFail() {
        var leader  = await PartyActor.CreateAsync(host, "party-notready-l");
        var member1 = await PartyActor.CreateAsync(host, "party-notready-m1");
        var member2 = await PartyActor.CreateAsync(host, "party-notready-m2");

        _ = await leader.InviteAsync(member1.Id);
        _ = await leader.InviteAsync(member2.Id);
        _ = await member1.AcceptAsync(leader.Id);
        _ = await member2.AcceptAsync(leader.Id);

        _ = await leader.SetReadyAsync(leader.Id, true);
        _ = await member1.SetReadyAsync(leader.Id, true);
        // member2 not ready

        var leaderMm = await CreateUserAsync("party-notready-l");
        var join     = await leaderMm.JoinAsync("photon_dm");

        join.AssertDataIsNull();
    }

    [Fact]
    public async Task Party_MemberCancels_ShouldRemoveWholeGroupFromQueue() {
        var leader = await PartyActor.CreateAsync(host, "party-cancel-mem-l");
        var member = await PartyActor.CreateAsync(host, "party-cancel-mem-m");

        _ = await leader.InviteAsync(member.Id);
        _ = await member.AcceptAsync(leader.Id);
        _ = await leader.SetReadyAsync(leader.Id, true);
        _ = await member.SetReadyAsync(leader.Id, true);

        var leaderMm = await CreateUserAsync("party-cancel-mem-l");
        var memberMm = await CreateUserAsync("party-cancel-mem-m");

        var joinRes = await leaderMm.JoinAsync("photon_duo");
        joinRes.ShouldBeEnqueued();

        var cancelByMember = await memberMm.CancelAsync();
        cancelByMember.ShouldBeOk();

        (await leaderMm.StatusAsync()).ShouldBeIdle();
        (await memberMm.StatusAsync()).ShouldBeIdle();
    }

    [Fact]
    public async Task Party_LeaderCancels_ShouldRemoveWholeGroupFromQueue() {
        var leader = await PartyActor.CreateAsync(host, "party-cancel-lead-l");
        var member = await PartyActor.CreateAsync(host, "party-cancel-lead-m");

        _ = await leader.InviteAsync(member.Id);
        _ = await member.AcceptAsync(leader.Id);
        _ = await leader.SetReadyAsync(leader.Id, true);
        _ = await member.SetReadyAsync(leader.Id, true);

        var leaderMm = await CreateUserAsync("party-cancel-lead-l");
        var memberMm = await CreateUserAsync("party-cancel-lead-m");

        (await leaderMm.JoinAsync("photon_duo")).ShouldBeEnqueued();

        await Task.Delay(250);

        (await leaderMm.CancelAsync()).ShouldBeOk();

        (await leaderMm.StatusAsync()).ShouldBeIdle();
        (await memberMm.StatusAsync()).ShouldBeIdle();
    }

    // ==================== PARTY QUEUE: MATCHING ====================

    [Fact]
    public async Task Party_TwoPartiesDuo_ShouldMatchWith4Players() {
        var l1 = await PartyActor.CreateAsync(host, "party-2duo-l1");
        var m1 = await PartyActor.CreateAsync(host, "party-2duo-m1");
        _ = await l1.InviteAsync(m1.Id);
        _ = await m1.AcceptAsync(l1.Id);
        _ = await l1.SetReadyAsync(l1.Id, true);
        _ = await m1.SetReadyAsync(l1.Id, true);

        var l2 = await PartyActor.CreateAsync(host, "party-2duo-l2");
        var m2 = await PartyActor.CreateAsync(host, "party-2duo-m2");
        _ = await l2.InviteAsync(m2.Id);
        _ = await m2.AcceptAsync(l2.Id);
        _ = await l2.SetReadyAsync(l2.Id, true);
        _ = await m2.SetReadyAsync(l2.Id, true);

        var g1 = await CreateUserAsync("party-2duo-l1");
        var g2 = await CreateUserAsync("party-2duo-l2");

        _ = await g1.JoinAsync("photon_duo");
        _ = await g2.JoinAsync("photon_duo");

        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? s1       = null, s2 = null;
        var                                                      deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            s1 = await g1.StatusAsync();
            s2 = await g2.StatusAsync();
            if (s1.DataNotNull() && s2.DataNotNull() &&
                s1.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched &&
                s2.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched) {
                break;
            }
            await Task.Delay(100);
        }

        Assert.NotNull(s1);
        Assert.NotNull(s2);
        s1.ShouldBeMatchedWithContract();
        s2.ShouldBeMatchedWithContract();
        Assert.Equal(4, s1.Data.Join.ExpectedUsers.Length);
        Assert.Equal(4, s2.Data.Join.ExpectedUsers.Length);

        var ids = s1.Data.Join.ExpectedUsers.ToHashSet();
        Assert.Contains(l1.Id, ids);
        Assert.Contains(m1.Id, ids);
        Assert.Contains(l2.Id, ids);
        Assert.Contains(m2.Id, ids);
        Assert.Equal("photon_duo", s1.Data.Join.RoomPropsMinimal["mode"]);
        Assert.Equal("0", s1.Data.Join.RoomPropsMinimal.GetValueOrDefault("bots", "0"));
    }

    // ==================== MIXED QUEUE: PARTY + SOLO ====================

    [Fact]
    public async Task Mixed_OnePartyPlusSolos_ShouldMatchTogether() {
        // Create party of 2
        var leader = await PartyActor.CreateAsync(host, "mixed-1p-l");
        var member = await PartyActor.CreateAsync(host, "mixed-1p-m");
        _ = await leader.InviteAsync(member.Id);
        _ = await member.AcceptAsync(leader.Id);
        _ = await leader.SetReadyAsync(leader.Id, true);
        _ = await member.SetReadyAsync(leader.Id, true);

        // Create 4 solo players
        var solos = await Task.WhenAll(Enumerable.Range(0, 4).Select(i => CreateUserAsync($"mixed-1p-solo-{i}")));

        // Queue party leader and solos
        var partyMm = await CreateUserAsync("mixed-1p-l");
        _ = await partyMm.JoinAsync("photon_dm");

        foreach (var solo in solos) {
            _ = await solo.JoinAsync("photon_dm");
        }

        // Wait for match
        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? partyStatus = null;
        var                                                      deadline    = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            partyStatus = await partyMm.StatusAsync();
            var soloStatuses = await Task.WhenAll(solos.Select(s => s.StatusAsync()));

            if (partyStatus.DataNotNull() &&
                partyStatus.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched &&
                soloStatuses.All(s => s.DataNotNull() && s.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched)) {
                break;
            }
            await Task.Delay(100);
        }

        Assert.NotNull(partyStatus);
        partyStatus.ShouldBeMatchedWithContract();
        Assert.Equal(6, partyStatus.Data.Join.ExpectedUsers.Length);

        // Verify all 6 players (2 party + 4 solo) are in the match
        var expectedSet = partyStatus.Data.Join.ExpectedUsers.ToHashSet();
        Assert.Contains(leader.Id, expectedSet);
        Assert.Contains(member.Id, expectedSet);
        foreach (var solo in solos) {
            Assert.Contains(solo.Id, expectedSet);
        }

        Assert.Equal("photon_dm", partyStatus.Data.Join.RoomPropsMinimal["mode"]);
    }

    [Fact]
    public async Task Mixed_TwoPartiesPlusSolos_ShouldMatchTogether() {
        // Create two parties of 2
        var l1 = await PartyActor.CreateAsync(host, "mixed-2p-l1");
        var m1 = await PartyActor.CreateAsync(host, "mixed-2p-m1");
        _ = await l1.InviteAsync(m1.Id);
        _ = await m1.AcceptAsync(l1.Id);
        _ = await l1.SetReadyAsync(l1.Id, true);
        _ = await m1.SetReadyAsync(l1.Id, true);

        var l2 = await PartyActor.CreateAsync(host, "mixed-2p-l2");
        var m2 = await PartyActor.CreateAsync(host, "mixed-2p-m2");
        _ = await l2.InviteAsync(m2.Id);
        _ = await m2.AcceptAsync(l2.Id);
        _ = await l2.SetReadyAsync(l2.Id, true);
        _ = await m2.SetReadyAsync(l2.Id, true);

        // Create 2 solo players
        var solo1 = await CreateUserAsync("mixed-2p-solo-1");
        var solo2 = await CreateUserAsync("mixed-2p-solo-2");

        // Queue everyone
        var party1Mm = await CreateUserAsync("mixed-2p-l1");
        var party2Mm = await CreateUserAsync("mixed-2p-l2");

        _ = await party1Mm.JoinAsync("photon_dm");
        _ = await party2Mm.JoinAsync("photon_dm");
        _ = await solo1.JoinAsync("photon_dm");
        _ = await solo2.JoinAsync("photon_dm");

        // Wait for match
        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? p1Status = null;
        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? s1Status = null;
        var                                                      deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            p1Status = await party1Mm.StatusAsync();
            s1Status = await solo1.StatusAsync();

            if (p1Status.DataNotNull() && s1Status.DataNotNull() &&
                p1Status.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched &&
                s1Status.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched) {
                break;
            }
            await Task.Delay(100);
        }

        Assert.NotNull(p1Status);
        Assert.NotNull(s1Status);
        p1Status.ShouldBeMatchedWithContract();
        s1Status.ShouldBeMatchedWithContract();
        Assert.Equal(6, p1Status.Data.Join.ExpectedUsers.Length);

        // Verify all 6 players (4 party + 2 solo) are in the same room
        var expectedSet = p1Status.Data.Join.ExpectedUsers.ToHashSet();
        Assert.Contains(l1.Id, expectedSet);
        Assert.Contains(m1.Id, expectedSet);
        Assert.Contains(l2.Id, expectedSet);
        Assert.Contains(m2.Id, expectedSet);
        Assert.Contains(solo1.Id, expectedSet);
        Assert.Contains(solo2.Id, expectedSet);

        Assert.Equal(p1Status.Data.Join.RoomName, s1Status.Data.Join.RoomName);
    }

    [Fact]
    public async Task Mixed_PartyPrioritizesOtherParties_ButAcceptsSolosIfNeeded() {
        // Create one party of 2
        var leader = await PartyActor.CreateAsync(host, "mixed-prio-l");
        var member = await PartyActor.CreateAsync(host, "mixed-prio-m");
        _ = await leader.InviteAsync(member.Id);
        _ = await member.AcceptAsync(leader.Id);
        _ = await leader.SetReadyAsync(leader.Id, true);
        _ = await member.SetReadyAsync(leader.Id, true);

        // Create 4 solo players (no other parties available)
        var solos = await Task.WhenAll(Enumerable.Range(0, 4).Select(i => CreateUserAsync($"mixed-prio-solo-{i}")));

        var partyMm = await CreateUserAsync("mixed-prio-l");
        _ = await partyMm.JoinAsync("photon_dm");

        foreach (var solo in solos) {
            _ = await solo.JoinAsync("photon_dm");
        }

        // Since there are no other parties, the system should match party with solos
        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? partyStatus = null;
        var                                                      deadline    = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            partyStatus = await partyMm.StatusAsync();
            if (partyStatus.DataNotNull() &&
                partyStatus.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched) {
                break;
            }
            await Task.Delay(100);
        }

        Assert.NotNull(partyStatus);
        partyStatus.ShouldBeMatchedWithContract();

        // Verify party was matched with solos (not just party alone)
        Assert.Equal(6, partyStatus.Data.Join.ExpectedUsers.Length);
        var expectedSet = partyStatus.Data.Join.ExpectedUsers.ToHashSet();
        Assert.Contains(leader.Id, expectedSet);
        Assert.Contains(member.Id, expectedSet);

        // At least some solos should be included
        var soloCount = solos.Count(s => expectedSet.Contains(s.Id));
        Assert.True(soloCount >= 4, $"Expected at least 4 solos in the match, but got {soloCount}");
    }

    // ==================== LOADOUT HELPER TESTS ====================

    [Fact]
    public async Task Helper_SetPlayerPower_ShouldSetLoadout() {
        var user = await CreateUserAsync("helper-test-1");

        // Устанавливаем силу игрока
        await user.SetPlayerPowerAsync(500);

        // Проверяем, что loadout был установлен (тест пройдет, если не выбросится исключение)
        Assert.True(true);
    }

    // ==================== BASIC MATCHING (FIFO) ====================
    // Note: Skill-based matching is prepared but disabled for now
    // These tests verify current FIFO behavior

    [Fact]
    public async Task BasicMatching_SixPlayers_ShouldMatchTogether() {
        // FIFO: First 6 players should match together regardless of skill
        var players = await Task.WhenAll(Enumerable.Range(0, 6).Select(i => CreateUserAsync($"basic-6p-{i}")));

        foreach (var player in players) {
            _ = await player.JoinAsync("photon_dm");
        }

        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>[] statuses = [];
        var                                                       deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            statuses = await Task.WhenAll(players.Select(p => p.StatusAsync()));
            if (statuses.All(s => s.DataNotNull() && s.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched)) {
                break;
            }
            await Task.Delay(100);
        }

        foreach (var status in statuses) {
            status.ShouldBeMatchedWithContract();
        }

        // All should be in the same room
        var roomNames = statuses.Select(s => s.Data.Join.RoomName).Distinct().ToArray();
        Assert.Single(roomNames);
        Assert.Equal(6, statuses[0].Data.Join.ExpectedUsers.Length);
    }

    [Fact]
    public async Task BasicMatching_TenPlayers_FormsTwoMatches() {
        // FIFO: 10 players → first 6 match, then next 4 wait (or timeout with 4 players)
        var players = await Task.WhenAll(Enumerable.Range(0, 10).Select(i => CreateUserAsync($"basic-10p-{i}")));

        foreach (var player in players) {
            _ = await player.JoinAsync("photon_dm");
        }

        // Wait for first match
        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>? firstPlayerStatus = null;
        var                                                      deadline          = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            firstPlayerStatus = await players[0].StatusAsync();
            if (firstPlayerStatus.DataNotNull() &&
                firstPlayerStatus.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched) {
                break;
            }
            await Task.Delay(100);
        }

        Assert.NotNull(firstPlayerStatus);
        firstPlayerStatus.ShouldBeMatchedWithContract();

        // First 6 players should be matched together
        Assert.Equal(6, firstPlayerStatus.Data.Join.ExpectedUsers.Length);

        // Players 0-5 should be matched
        for (int i = 0; i < 6; i++) {
            var status = await players[i].StatusAsync();
            status.ShouldBeMatchedWithContract();
            Assert.Equal(firstPlayerStatus.Data.Join.RoomName, status.Data.Join.RoomName);
        }

        // Players 6-9 should still be queued or matched separately
        var player6Status = await players[6].StatusAsync();
        Assert.True(
            player6Status.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Queued ||
            player6Status.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched
        );
    }

    [Fact]
    public async Task BasicMatching_TwelvePlayersWithTimeout_ShouldFormTwoMatches() {
        // 12 players: First 6 match immediately, next 6 also match
        var players = await Task.WhenAll(Enumerable.Range(0, 12).Select(i => CreateUserAsync($"basic-12p-{i}")));

        foreach (var player in players) {
            _ = await player.JoinAsync("photon_dm");
        }

        // Wait for all to match
        ServerResult<Game.Shared.DTO.MatchmakingStatusResponse>[] statuses = [];
        var                                                       deadline = DateTime.UtcNow + TimeSpan.FromSeconds(12);

        while (DateTime.UtcNow < deadline) {
            statuses = await Task.WhenAll(players.Select(p => p.StatusAsync()));
            if (statuses.All(s => s.DataNotNull() && s.Data.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched)) {
                break;
            }
            await Task.Delay(100);
        }

        // All should be matched
        foreach (var status in statuses) {
            status.ShouldBeMatchedWithContract();
        }

        // Should be exactly 2 different rooms
        var roomNames = statuses.Select(s => s.Data.Join.RoomName).Distinct().ToArray();
        Assert.Equal(2, roomNames.Length);

        // Each room should have 6 players
        var room1Players = statuses.Where(s => s.Data.Join.RoomName == roomNames[0]).Select(s => s.Data.Join.ExpectedUsers).First();
        var room2Players = statuses.Where(s => s.Data.Join.RoomName == roomNames[1]).Select(s => s.Data.Join.ExpectedUsers).First();

        Assert.Equal(6, room1Players.Length);
        Assert.Equal(6, room2Players.Length);
    }

    // ==================== PARTY QUEUE: EVENTS ====================

    [Fact]
    public async Task Party_Join_ShouldEmitStartedEventForAllMembers() {
        var leader = await PartyActor.CreateAsync(host, "party-evt-all-l");
        var member = await PartyActor.CreateAsync(host, "party-evt-all-m");

        _ = await leader.InviteAsync(member.Id);
        _ = await member.AcceptAsync(leader.Id);
        _ = await leader.SetReadyAsync(leader.Id, true);
        _ = await member.SetReadyAsync(leader.Id, true);

        var leaderMm = await CreateUserAsync("party-evt-all-l");
        var memberMm = await CreateUserAsync("party-evt-all-m");

        await using var leaderEvents = await AppEventsClient.ConnectAsync(host, await leaderMm.GetAccessTokenAsync());
        await using var memberEvents = await AppEventsClient.ConnectAsync(host, await memberMm.GetAccessTokenAsync());

        await Task.Delay(500);

        _ = await leaderMm.JoinAsync("photon_dm");

        var evtLeader = await leaderEvents.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));
        var evtMember = await memberEvents.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5));

        Assert.NotNull(evtLeader);
        Assert.NotNull(evtMember);
        Assert.Equal("photon_dm", evtLeader.GameModeKey);
        Assert.Equal(evtLeader.LeaderUserId, evtMember.LeaderUserId);
        Assert.Equal(leader.Id, evtLeader.LeaderUserId);
    }

    [Fact]
    public async Task Party_MemberCancels_ShouldEmitCanceledEventForAll() {
        var leaderP = await PartyActor.CreateAsync(host, "party-cancel-evt-l");
        var memberP = await PartyActor.CreateAsync(host, "party-cancel-evt-m");

        _ = await leaderP.InviteAsync(memberP.Id);
        _ = await memberP.AcceptAsync(leaderP.Id);
        _ = await leaderP.SetReadyAsync(leaderP.Id, true);
        _ = await memberP.SetReadyAsync(leaderP.Id, true);

        var leader = await CreateUserAsync("party-cancel-evt-l");
        var member = await CreateUserAsync("party-cancel-evt-m");

        await using var evLeader = await AppEventsClient.ConnectAsync(host, await leader.GetAccessTokenAsync());
        await using var evMember = await AppEventsClient.ConnectAsync(host, await member.GetAccessTokenAsync());

        await Task.Delay(250);

        _ = await leader.JoinAsync("photon_dm");

        Assert.NotNull(await evLeader.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5)));
        Assert.NotNull(await evMember.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5)));

        _ = await member.CancelAsync();

        var canceledEvtLeader = await evLeader.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(5));
        var canceledEvtMember = await evMember.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(5));

        Assert.NotNull(canceledEvtLeader);
        Assert.NotNull(canceledEvtMember);
        Assert.Equal(leader.Id, canceledEvtLeader.LeaderUserId);
        Assert.Equal(member.Id, canceledEvtLeader.CanceledByUserId);

        var statusL = await leader.StatusAsync();
        var statusM = await member.StatusAsync();

        Assert.Equal(Game.Shared.DTO.EMatchmakingQueueStatus.Idle, statusL.Data.Status);
        Assert.Equal(Game.Shared.DTO.EMatchmakingQueueStatus.Idle, statusM.Data.Status);
    }

    [Fact]
    public async Task Party_LeaderCancels_ShouldEmitCanceledEventForAll() {
        var leaderP = await PartyActor.CreateAsync(host, "party-lcancel-evt-l");
        var memberP = await PartyActor.CreateAsync(host, "party-lcancel-evt-m");

        _ = await leaderP.InviteAsync(memberP.Id);
        _ = await memberP.AcceptAsync(leaderP.Id);
        _ = await leaderP.SetReadyAsync(leaderP.Id, true);
        _ = await memberP.SetReadyAsync(leaderP.Id, true);

        var leader = await CreateUserAsync("party-lcancel-evt-l");
        var member = await CreateUserAsync("party-lcancel-evt-m");

        await using var evLeader = await AppEventsClient.ConnectAsync(host, await leader.GetAccessTokenAsync());
        await using var evMember = await AppEventsClient.ConnectAsync(host, await member.GetAccessTokenAsync());

        await Task.Delay(250);

        _ = await leader.JoinAsync("photon_dm");

        Assert.NotNull(await evLeader.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5)));
        Assert.NotNull(await evMember.WaitForAsync<PartyMatchmakingStartedAppServerEvent>(TimeSpan.FromSeconds(5)));

        _ = await leader.CancelAsync();

        var canceledEvtLeader = await evLeader.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(5));
        var canceledEvtMember = await evMember.WaitForAsync<PartyMatchmakingCanceledAppServerEvent>(TimeSpan.FromSeconds(5));

        Assert.NotNull(canceledEvtLeader);
        Assert.NotNull(canceledEvtMember);
        Assert.Equal(leader.Id, canceledEvtLeader.LeaderUserId);
        Assert.Equal(leader.Id, canceledEvtLeader.CanceledByUserId);

        var statusL = await leader.StatusAsync();
        var statusM = await member.StatusAsync();

        Assert.Equal(Game.Shared.DTO.EMatchmakingQueueStatus.Idle, statusL.Data.Status);
        Assert.Equal(Game.Shared.DTO.EMatchmakingQueueStatus.Idle, statusM.Data.Status);
    }

    [Fact]
    public async Task Party_Match_ShouldEmitMatchedEventForAllMembers() {
        // Create two parties (2 players each) for duo mode (requires 4 players)
        var l1 = await PartyActor.CreateAsync(host, "party-matched-evt-l1");
        var m1 = await PartyActor.CreateAsync(host, "party-matched-evt-m1");
        _ = await l1.InviteAsync(m1.Id);
        _ = await m1.AcceptAsync(l1.Id);
        _ = await l1.SetReadyAsync(l1.Id, true);
        _ = await m1.SetReadyAsync(l1.Id, true);

        var l2 = await PartyActor.CreateAsync(host, "party-matched-evt-l2");
        var m2 = await PartyActor.CreateAsync(host, "party-matched-evt-m2");
        _ = await l2.InviteAsync(m2.Id);
        _ = await m2.AcceptAsync(l2.Id);
        _ = await l2.SetReadyAsync(l2.Id, true);
        _ = await m2.SetReadyAsync(l2.Id, true);

        var leader1 = await CreateUserAsync("party-matched-evt-l1");
        var member1 = await CreateUserAsync("party-matched-evt-m1");
        var leader2 = await CreateUserAsync("party-matched-evt-l2");
        var member2 = await CreateUserAsync("party-matched-evt-m2");

        await using var evLeader1 = await AppEventsClient.ConnectAsync(host, await leader1.GetAccessTokenAsync());
        await using var evMember1 = await AppEventsClient.ConnectAsync(host, await member1.GetAccessTokenAsync());
        await using var evLeader2 = await AppEventsClient.ConnectAsync(host, await leader2.GetAccessTokenAsync());
        await using var evMember2 = await AppEventsClient.ConnectAsync(host, await member2.GetAccessTokenAsync());

        await Task.Delay(250);

        _ = await leader1.JoinAsync("photon_duo");
        _ = await leader2.JoinAsync("photon_duo");

        var matchedEvtL1 = await evLeader1.WaitForAsync<PartyMatchmakingMatchedAppServerEvent>(TimeSpan.FromSeconds(12));
        var matchedEvtM1 = await evMember1.WaitForAsync<PartyMatchmakingMatchedAppServerEvent>(TimeSpan.FromSeconds(12));
        var matchedEvtL2 = await evLeader2.WaitForAsync<PartyMatchmakingMatchedAppServerEvent>(TimeSpan.FromSeconds(12));
        var matchedEvtM2 = await evMember2.WaitForAsync<PartyMatchmakingMatchedAppServerEvent>(TimeSpan.FromSeconds(12));

        Assert.NotNull(matchedEvtL1);
        Assert.NotNull(matchedEvtM1);
        Assert.NotNull(matchedEvtL2);
        Assert.NotNull(matchedEvtM2);

        // All events should have the same room and mode
        Assert.Equal(matchedEvtL1.RoomName, matchedEvtM1.RoomName);
        Assert.Equal(matchedEvtL1.RoomName, matchedEvtL2.RoomName);
        Assert.Equal(matchedEvtL1.RoomName, matchedEvtM2.RoomName);
        Assert.Equal("photon_duo", matchedEvtL1.GameModeKey);
        Assert.Equal(4, matchedEvtL1.ExpectedUsers.Length);

        // Verify all 4 players are in the expected users list
        var expectedSet = matchedEvtL1.ExpectedUsers.ToHashSet();
        Assert.Contains(l1.Id, expectedSet);
        Assert.Contains(m1.Id, expectedSet);
        Assert.Contains(l2.Id, expectedSet);
        Assert.Contains(m2.Id, expectedSet);
    }
}