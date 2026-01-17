namespace Game.ServerRunner.ApiControllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.DTO;
using Grains;
using Multicast;
[ApiController]
[Produces(ServerConstants.CONTENT_TYPE_MSGPACK)]
public class PartyApiController(IGrainFactory grainFactory) : Controller {
    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.INVITE)]
    public async Task<ServerResult<PartyInviteResponse>> Invite(PartyInviteRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null || request.TargetUserId == Guid.Empty) {
            return ServerResult.Error(2, "Invalid request");
        }

        var party = grainFactory.GetGrain<IPartyGrain>(userId);
        return await party.Invite(userId, request.TargetUserId);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.ACCEPT)]
    public async Task<ServerResult<PartyAcceptInviteResponse>> Accept(PartyAcceptInviteRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null || request.LeaderUserId == Guid.Empty) {
            return ServerResult.Error(2, "Invalid request");
        }

        var party = grainFactory.GetGrain<IPartyGrain>(request.LeaderUserId);

        return await party.Accept(request.LeaderUserId, userId);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.DECLINE)]
    public async Task<ServerResult<PartyDeclineInviteResponse>> Decline(PartyDeclineInviteRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null || request.LeaderUserId == Guid.Empty) {
            return ServerResult.Error(2, "Invalid request");
        }

        var party = grainFactory.GetGrain<IPartyGrain>(request.LeaderUserId);

        return await party.Decline(request.LeaderUserId, userId);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.LEAVE)]
    public async Task<ServerResult<PartyLeaveResponse>> Leave(PartyLeaveRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null || request.LeaderUserId == Guid.Empty) {
            return ServerResult.Error(2, "Invalid request");
        }

        var party = grainFactory.GetGrain<IPartyGrain>(request.LeaderUserId);

        return await party.Leave(request.LeaderUserId, userId);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.START)]
    public async Task<ServerResult<PartyStartGameResponse>> Start(PartyStartGameRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null || string.IsNullOrWhiteSpace(request.GameModeKey)) {
            return ServerResult.Error(2, "Invalid request");
        }

        var mapping = grainFactory.GetUserPartyGrain(userId);
        var leader  = await mapping.GetLeader();

        if (leader == Guid.Empty || leader != userId) {
            return ServerResult.Error(3, "Only leader can start");
        }

        var party = grainFactory.GetGrain<IPartyGrain>(leader);

        return await party.StartGame(leader, request.GameModeKey);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.STATUS)]
    public async Task<ServerResult<PartyStatusResponse>> Status(PartyStatusRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        var mapping = grainFactory.GetUserPartyGrain(userId);
        var leader  = await mapping.GetLeader();

        if (leader == Guid.Empty) {
            return new PartyStatusResponse { LeaderUserId = Guid.Empty, Members = Array.Empty<Guid>(), ReadyMembers = Array.Empty<Guid>() };
        }

        var party = grainFactory.GetGrain<IPartyGrain>(leader);

        var resp = await party.GetStatus();

        resp.ReadyMembers ??= Array.Empty<Guid>();

        return resp;
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.READY_SET)]
    public async Task<ServerResult<PartySetReadyResponse>> SetReady(PartySetReadyRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null || request.LeaderUserId == Guid.Empty) {
            return ServerResult.Error(2, "Invalid request");
        }

        var party = grainFactory.GetGrain<IPartyGrain>(request.LeaderUserId);
        return await party.SetReady(request.LeaderUserId, userId, request.IsReady);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.KICK)]
    public async Task<ServerResult<PartyKickResponse>> Kick(PartyKickRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null || request.LeaderUserId == Guid.Empty || request.TargetUserId == Guid.Empty) {
            return ServerResult.Error(2, "Invalid request");
        }

        if (request.LeaderUserId != userId) {
            return ServerResult.Error(3, "Only leader can kick");
        }

        var party = grainFactory.GetGrain<IPartyGrain>(request.LeaderUserId);

        return await party.Kick(request.LeaderUserId, request.TargetUserId);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Party.MAKE_LEADER)]
    public async Task<ServerResult<PartyMakeLeaderResponse>> MakeLeader(PartyMakeLeaderRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null) {
            return ServerResult.Error(3, "Invalid request");
        }

        if (request.LeaderUserId == Guid.Empty) {
            return ServerResult.Error(4, "Invalid request");
        }

        if (request.TargetUserId == Guid.Empty) {
            return ServerResult.Error(5, "Invalid request");
        }

        if (request.LeaderUserId != userId) {
            return ServerResult.Error(6, "Only leader can transfer leadership");
        }

        var party = grainFactory.GetGrain<IPartyGrain>(request.LeaderUserId);

        return await party.MakeLeader(request.LeaderUserId, request.TargetUserId);
    }
}