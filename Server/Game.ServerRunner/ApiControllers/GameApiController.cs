namespace Game.ServerRunner.ApiControllers;

using Grains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multicast;
using Shared;
using Shared.DTO;

[ApiController]
[Produces(ServerConstants.CONTENT_TYPE_MSGPACK)]
public class GameApiController(IGrainFactory grainFactory) : Controller {
    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Game.REPORT_GAME_SNAPSHOT)]
    public async Task<ServerResult<ReportGameSnapshotResponse>> AuthGuest(ReportGameSnapshotRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (string.IsNullOrEmpty(request.GameId)) {
            return ServerResult.Error(2, "GameID is empty");
        }

        // TODO validate game id

        var userProfileGrain = grainFactory.GetUserProfileGrain(userId);

        return await grainFactory
            .GetGameGrain(request.GameId)
            .ReportGameSnapshot(userProfileGrain, request);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Game.REPORT_QUEST_COUNTER_TASK)]
    public async Task<ServerResult<ReportGameQuestCounterTaskResponse>> ReportQuestCounterTask(
        ReportGameQuestCounterTaskRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        if (string.IsNullOrEmpty(request.GameId)) {
            return ServerResult.Error(2, "GameID is empty");
        }

        // TODO validate game id

        var userProfileGrain = grainFactory.GetUserProfileGrain(userId);

        return await grainFactory
            .GetGameGrain(request.GameId)
            .ReportCounterTask(userProfileGrain, request);
    }
}