namespace Game.ServerRunner.Grains;

using Microsoft.Extensions.Logging;
using Multicast;
using Orleans;
using Shared;
using Shared.DTO;
using Shared.ServerEvents;
public interface IPartyGrain : IGrainWithGuidKey {
    ValueTask<ServerResult<PartyInviteResponse>>        Invite(Guid inviterUserId, Guid targetUserId);
    ValueTask<ServerResult<PartyAcceptInviteResponse>>  Accept(Guid leaderUserId, Guid acceptorUserId);
    ValueTask<ServerResult<PartyDeclineInviteResponse>> Decline(Guid leaderUserId, Guid declinerUserId);
    ValueTask<ServerResult<PartyLeaveResponse>>         Leave(Guid leaderUserId, Guid leaverUserId);
    ValueTask<ServerResult<PartyKickResponse>>          Kick(Guid leaderUserId, Guid targetUserId);
    ValueTask<ServerResult<PartyMakeLeaderResponse>>    MakeLeader(Guid leaderUserId, Guid targetUserId);
    ValueTask<ServerResult<PartyStartGameResponse>>     StartGame(Guid leaderUserId, string gameModeKey);
    ValueTask                                           AdoptFrom(Guid oldLeaderUserId, Guid[] memberList, Guid[] inviteList);
    ValueTask<PartyStatusResponse>                      GetStatus();
    ValueTask<ServerResult<PartySetReadyResponse>>      SetReady(Guid leaderUserId, Guid userId, bool isReady);
    ValueTask                                           OnMemberEnteredGame(Guid userId);
}
public static class PartyGrainExtensions {
    public static Guid        GetPartyId(this IPartyGrain grain)                           => grain.GetPrimaryKey();
    public static IPartyGrain GetPartyGrain(this IGrainFactory factory, Guid leaderUserId) => factory.GetGrain<IPartyGrain>(leaderUserId);
}
public sealed class PartyGrain(IGrainFactory grainFactory, ILogger<PartyGrain> logger) : Grain, IPartyGrain {
    private readonly HashSet<Guid> members = new();
    private readonly HashSet<Guid> invites = new();
    private readonly HashSet<Guid> ready   = new();
    private          bool          isInitialized;

    private Guid LeaderId => this.GetPrimaryKey();

    public override async Task OnActivateAsync(CancellationToken cancellationToken) {
        if (!isInitialized) {
            members.Clear();
            invites.Clear();
            ready.Clear();
            members.Add(LeaderId);
            await grainFactory.GetUserPartyGrain(LeaderId).TryJoin(LeaderId);
            isInitialized = true;
        }
    }

    public async ValueTask<ServerResult<PartyInviteResponse>> Invite(Guid inviterUserId, Guid targetUserId) {
        if (inviterUserId != LeaderId) {
            return ServerResult.Error(1, "Only leader can invite");
        }
        if (targetUserId == Guid.Empty || targetUserId == inviterUserId) {
            return ServerResult.Error(2, "Invalid target");
        }

        if (members.Contains(targetUserId)) {
            return new PartyInviteResponse { Result = EPartyInviteActionStatus.AlreadyInParty };
        }
        if (!invites.Add(targetUserId)) {
            return new PartyInviteResponse { Result = EPartyInviteActionStatus.AlreadyInvited };
        }

        await PublishToUser(targetUserId, new PartyInviteReceivedAppServerEvent { LeaderUserId = LeaderId });
        await PublishPartyUpdated();

        return new PartyInviteResponse { Result = EPartyInviteActionStatus.Sent };
    }

    public async ValueTask<ServerResult<PartyAcceptInviteResponse>> Accept(Guid leaderUserId, Guid acceptorUserId) {
        if (leaderUserId != LeaderId) {
            return ServerResult.Error(1, "Invalid party");
        }
        if (!invites.Remove(acceptorUserId)) {
            return ServerResult.Error(2, "No invite");
        }

        var mapping   = grainFactory.GetUserPartyGrain(acceptorUserId);
        var oldLeader = await mapping.GetLeader();
        if (oldLeader != Guid.Empty && oldLeader != LeaderId) {
            var oldParty = grainFactory.GetGrain<IPartyGrain>(oldLeader);
            await oldParty.Leave(oldLeader, acceptorUserId);
        }
        await mapping.TryJoin(LeaderId);
        members.Add(acceptorUserId);
        ready.Remove(acceptorUserId);
        await PublishPartyUpdated();
        return new PartyAcceptInviteResponse();
    }

    public async ValueTask<ServerResult<PartyDeclineInviteResponse>> Decline(Guid leaderUserId, Guid declinerUserId) {
        if (leaderUserId != LeaderId) {
            return ServerResult.Error(1, "Invalid party");
        }
        invites.Remove(declinerUserId);
        await PublishPartyUpdated();
        return new PartyDeclineInviteResponse();
    }

    public async ValueTask<ServerResult<PartyLeaveResponse>> Leave(Guid leaderUserId, Guid leaverUserId) {
        if (leaderUserId != LeaderId) {
            return ServerResult.Error(1, "Invalid party");
        }

        if (!members.Remove(leaverUserId)) {
            return ServerResult.Error(2, "Not in party");
        }
        ready.Remove(leaverUserId);

        await grainFactory.GetUserPartyGrain(leaverUserId).Leave();

        var evtAfterLeave = new PartyUpdatedAppServerEvent { LeaderUserId = LeaderId, Members = members.ToArray(), ReadyMembers = ready.ToArray() };

        await PublishToUser(leaverUserId, evtAfterLeave);
        await PublishPartyUpdated();

        if (leaverUserId == LeaderId) {
            var remaining = members.ToArray();
            if (remaining.Length >= 2) {
                var newLeader = remaining.Min();
                var newParty  = grainFactory.GetGrain<IPartyGrain>(newLeader);

                var invitesCopy = invites.ToArray();

                members.Clear();
                invites.Clear();
                ready.Clear();
                isInitialized = false;

                await newParty.AdoptFrom(LeaderId, remaining, invitesCopy);

                DeactivateOnIdle();

                return new PartyLeaveResponse();
            }

            foreach (var uid in remaining) {
                await grainFactory.GetUserPartyGrain(uid).Leave();
            }

            await PublishDisbanded();

            members.Clear();
            invites.Clear();
            ready.Clear();
            isInitialized = false;

            DeactivateOnIdle();
        }
        return new PartyLeaveResponse();
    }

    public async ValueTask<ServerResult<PartyKickResponse>> Kick(Guid leaderUserId, Guid targetUserId) {
        if (leaderUserId != LeaderId) {
            return ServerResult.Error(1, "Only leader can kick");
        }

        if (targetUserId == LeaderId) {
            return ServerResult.Error(2, "Leader cannot kick self");
        }

        if (!members.Remove(targetUserId)) {
            return ServerResult.Error(3, "Not in party");
        }

        ready.Remove(targetUserId);

        await grainFactory.GetUserPartyGrain(targetUserId).Leave();

        var evtAfterLeave = new PartyUpdatedAppServerEvent { LeaderUserId = LeaderId, Members = members.ToArray(), ReadyMembers = ready.ToArray() };

        await PublishToUser(targetUserId, evtAfterLeave);
        await PublishPartyUpdated();

        return new PartyKickResponse();
    }

    public async ValueTask<ServerResult<PartyMakeLeaderResponse>> MakeLeader(Guid leaderUserId, Guid targetUserId) {
        if (leaderUserId != LeaderId) {
            return ServerResult.Error(1, "Only current leader can transfer leadership");
        }

        if (targetUserId == Guid.Empty || targetUserId == LeaderId) {
            return ServerResult.Error(2, "Invalid target");
        }

        if (!members.Contains(targetUserId)) {
            return ServerResult.Error(3, "Target not in party");
        }

        var oldLeader = LeaderId;
        var newLeader = targetUserId;

        var membersCopy = members.ToArray();
        var invitesCopy = invites.ToArray();

        members.Clear();
        invites.Clear();
        ready.Clear();
        isInitialized = false;

        var newParty = grainFactory.GetGrain<IPartyGrain>(newLeader);

        await newParty.AdoptFrom(oldLeader, membersCopy, invitesCopy);

        DeactivateOnIdle();

        return new PartyMakeLeaderResponse();
    }

    public async ValueTask AdoptFrom(Guid oldLeaderUserId, Guid[] memberList, Guid[] inviteList) {
        members.Clear();
        invites.Clear();
        ready.Clear();

        foreach (var m in memberList) {
            if (m != LeaderId) {
                members.Add(m);
            }
        }
        members.Add(LeaderId);

        foreach (var inv in inviteList) {
            invites.Add(inv);
        }

        foreach (var m in members) {
            var mapping = grainFactory.GetUserPartyGrain(m);
            await mapping.Leave();
            await mapping.TryJoin(LeaderId);
        }

        await PublishPartyUpdated();
    }

    public async ValueTask<ServerResult<PartyStartGameResponse>> StartGame(Guid leaderUserId, string gameModeKey) {
        if (leaderUserId != LeaderId) {
            return ServerResult.Error(1, "Only leader can start");
        }

        foreach (var m in members) {
            if (m == LeaderId) {
                continue;
            }

            if (!ready.Contains(m)) {
                return ServerResult.Error(4, "Not all members are ready");
            }
        }

        var gameId = Guid.NewGuid().ToString("N");

        foreach (var userId in members) {
            var userProfile = grainFactory.GetUserProfileGrain(userId);

            await userProfile.Execute(new Shared.UserProfile.Commands.Game.UserProfileJoinGameCommand { GameId = gameId }, UserProfileGrainExecuteOptions.SendUserProfileUpdatedEvent);
        }

        foreach (var userId in members) {
            await PublishToUser(userId, new PartyGameStartedAppServerEvent { LeaderUserId = LeaderId, GameId = gameId });
        }

        return new PartyStartGameResponse { GameId = gameId };
    }

    public ValueTask<PartyStatusResponse> GetStatus() {
        return ValueTask.FromResult(new PartyStatusResponse {
            LeaderUserId = LeaderId,
            Members      = members.ToArray(),
            ReadyMembers = ready.ToArray(),
        });
    }

    private async ValueTask PublishPartyUpdated() {
        var evt = new PartyUpdatedAppServerEvent { LeaderUserId = LeaderId, Members = members.ToArray(), ReadyMembers = ready.ToArray() };

        foreach (var userId in members) {
            await PublishToUser(userId, evt);
        }

        foreach (var userId in invites) {
            await PublishToUser(userId, evt);
        }
    }

    public async ValueTask<ServerResult<PartySetReadyResponse>> SetReady(Guid leaderUserId, Guid userId, bool isReady) {
        if (leaderUserId != LeaderId) {
            return ServerResult.Error(1, "Invalid party");
        }

        if (!members.Contains(userId)) {
            return ServerResult.Error(2, "Not in party");
        }

        var changed = false;

        if (isReady) {
            changed = ready.Add(userId);
        }

        else {
            changed = ready.Remove(userId);
        }
        if (changed) {
            await PublishPartyUpdated();
        }

        return new PartySetReadyResponse();
    }

    public async ValueTask OnMemberEnteredGame(Guid userId) {
        if (ready.Remove(userId)) {
            logger.LogInformation("Party: member {UserId} entered game, ready flag reset", userId);
            await PublishPartyUpdated();
        }
    }

    private async ValueTask PublishDisbanded() {
        var evt = new PartyDisbandedAppServerEvent { LeaderUserId = LeaderId };

        foreach (var userId in members.ToArray()) {
            await PublishToUser(userId, evt);
        }

        foreach (var userId in invites.ToArray()) {
            await PublishToUser(userId, evt);
        }
    }

    private async ValueTask PublishToUser(Guid targetUserId, IAppServerEvent evt) {
        await this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
            .GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(targetUserId))
            .OnNextAsync(evt);
    }
}