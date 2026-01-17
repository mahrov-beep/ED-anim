namespace Game.ServerRunner.ApiControllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multicast;
using Shared;
using Shared.DTO;
using Grains;
[ApiController]
[Produces(ServerConstants.CONTENT_TYPE_MSGPACK)]
public class MatchmakingApiController(IGrainFactory grainFactory) : Controller {
    [Authorize]
    [HttpPost("/api/matchmaking/join")]
    public async Task<ServerResult<MatchmakingJoinResponse>> Join(MatchmakingJoinRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (request == null || string.IsNullOrWhiteSpace(request.GameModeKey)) {
            return ServerResult.Error(2, "Invalid request");
        }

        var mm = grainFactory.GetGrain<IMatchmakingGrain>("all");

        return await mm.Join(userId, request.GameModeKey);
    }

    [Authorize]
    [HttpPost("/api/matchmaking/cancel")]
    public async Task<ServerResult<MatchmakingCancelResponse>> Cancel(MatchmakingCancelRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        var mm = grainFactory.GetGrain<IMatchmakingGrain>("all");

        return await mm.Cancel(userId);
    }

    [Authorize]
    [HttpPost("/api/matchmaking/status")]
    public async Task<ServerResult<MatchmakingStatusResponse>> Status(MatchmakingStatusRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        var mm = grainFactory.GetGrain<IMatchmakingGrain>("all");

        var res = await mm.Status(userId);

        return res;
    }

    [HttpPost("/api/matchmaking/__test_clear")]
    public async Task<IActionResult> ClearForTesting() {
        var env = Environment.GetEnvironmentVariable("ENV") ?? "prod";
        if (env != "dev") {
            return Forbid();
        }

        var mm = grainFactory.GetGrain<IMatchmakingGrain>("all");

        await mm.ClearForTesting();

        return Ok();
    }
}