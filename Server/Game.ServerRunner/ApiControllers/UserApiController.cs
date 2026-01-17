namespace Game.ServerRunner.ApiControllers;

using Grains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multicast;
using Shared;
using Shared.DTO;
using TestSetLoadoutCommand = Shared.UserProfile.Commands.Loadouts.TestSetLoadoutCommand;

[ApiController]
[Produces(ServerConstants.CONTENT_TYPE_MSGPACK)]
public class UserApiController(ILogger<UserApiController> logger, IGrainFactory grainFactory) : Controller {
    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.User.DELETE)]
    public async Task<ServerResult<UserDeleteResponse>> UserDelete(UserDeleteRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            logger.LogError("Received user profile request without userId claim. Ignore");
            this.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return ServerResult.Error(1, "User not exist");
        }

        return await grainFactory.GetUserProfileGrain(userId).DeleteUserProfile(request);
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.User.GET_INFO)]
    public async Task<ServerResult<UserGetInfoResponse>> UserGetInfo(UserGetInfoRequest request) {
        if (request is null || request.UserId == Guid.Empty) {
            return ServerResult.Error(1, "Invalid request");
        }

        var userProfile = grainFactory.GetUserProfileGrain(request.UserId);
        var nickName = await userProfile.GetNickName();
        var level = await userProfile.GetLevel();

        return new UserGetInfoResponse {
            NickName = nickName,
            Level = level,
        };
    }

    [HttpPost("/api/user/__test_set_loadout")]
    public async Task<ServerResult<TestSetLoadoutResponse>> TestSetLoadout(TestSetLoadoutRequest request) {
        var env = Environment.GetEnvironmentVariable("ENV");
        if (env != "dev") {
            return ServerResult.Error(1, "Not available");
        }

        if (request is null || request.UserId == Guid.Empty || request.Loadout == null) {
            return ServerResult.Error(2, "Invalid request");
        }

        try {
            var userProfile = grainFactory.GetUserProfileGrain(request.UserId);
            
            var testCommand = new TestSetLoadoutCommand {
                LoadoutSnapshot = request.Loadout,
            };

            var (code, error) = await userProfile.Execute(testCommand, UserProfileGrainExecuteOptions.None);

            if (code != ServerCommandResultCode.Ok) {
                logger.LogWarning("TestSetLoadout failed: {Error}", error);
                return ServerResult.Error(3, $"Failed to set loadout: {error}");
            }

            return new TestSetLoadoutResponse { Success = true };
        }
        catch (Exception ex) {
            logger.LogError(ex, "TestSetLoadout exception for UserId={UserId}", request.UserId);
            return ServerResult.Error(4, $"Exception: {ex.Message}");
        }
    }
}