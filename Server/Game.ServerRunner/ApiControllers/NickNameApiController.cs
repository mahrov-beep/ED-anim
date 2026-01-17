namespace Game.ServerRunner.ApiControllers;

using Grains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multicast;
using Shared;
using Shared.DTO;
[ApiController]
public class NickNameApiController(
                ILogger<UserProfileApiController> log,
                IGrainFactory grainFactory)
                : ControllerBase {

    [Authorize]
    [Produces(ServerConstants.CONTENT_TYPE_MSGPACK)]
    [HttpPost(SharedConstants.UrlRoutes.CHANGE_NICKNAME)]
    public async Task<ServerResult<ChangeNickNameResponse>> TryChangeNickname(ChangeNickNameRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            log.LogError("Received change nickname request without userId claim. Ignore");
            this.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return ServerResult.Error(1, "Invalid user id");
        }

        if (string.IsNullOrWhiteSpace(request?.newNick)) {
            return ServerResult.Error(2, "NickName must not be empty");
        }

        var userProfileGrain = grainFactory.GetUserProfileGrain(userId);

        var result = await userProfileGrain.Execute(new Shared.UserProfile.Commands.UserProfileSetNickNameCommand {
                        NewNickName = request.newNick,
        }, UserProfileGrainExecuteOptions.SendUserProfileUpdatedEvent);

        return new ChangeNickNameResponse {
                        success = result.code == ServerCommandResultCode.Ok,
        };
    }
}