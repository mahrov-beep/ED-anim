namespace Game.ServerRunner.ApiControllers;

using ServerRunner;
using Grains;
using Shared;
using Shared.UserProfile;
using MessagePack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Multicast;

public class UserProfileApiController(
    ILogger<UserProfileApiController> log,
    IGrainFactory grainFactory
) : Controller {
    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.USER_PROFILE)]
    [IgnoreAntiforgeryToken]
    public async Task Execute(
        [FromHeader(Name = "rid")] string requestId
    ) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            log.LogError("Received user profile request without userId claim. Ignore");
            this.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        IUserProfileServerCommand command = null;
        try {
            if (string.IsNullOrEmpty(requestId)) {
                log.LogError("Received user profile request without RequestId. Ignore");
                this.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            command = await MessagePackSerializer.DeserializeAsync<IUserProfileServerCommand>(this.HttpContext.Request.Body);

            if (command is null) {
                log.LogError("Received user profile request with null command. Ignore");
                this.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (command is not IServerCommandExecutableFromClient) {
                log.LogError("Received user profile request with command '{CommandType}' not marked as IServerCommandExecutableFromClient. Ignore", command.GetType().Name);
                this.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            log.LogInformation("Received user profile request (UserId={UserId}, CommandType={CommandType})",
                userId, command.GetType().Name
            );

            var userProfileGrain = grainFactory.GetUserProfileGrain(userId);
            var commandResult    = await userProfileGrain.Execute(command, UserProfileGrainExecuteOptions.None);

            if (commandResult.code != ServerCommandResultCode.Ok) {
                log.LogError("Failed user profile request (UserId={UserId}, CommandType={CommandType}, ResultCode={ResultCode}, Error={Error})",
                    userId, command.GetType().Name, commandResult.code, commandResult.error
                );

                // this.HttpContext.Response.StatusCode = commandResult.code == ServerCommandResultCode.BadRequest
                //     ? StatusCodes.Status400BadRequest
                //     : StatusCodes.Status500InternalServerError;

                this.HttpContext.Response.Headers["mc-err"] = commandResult.error;
                return;
            }

            var serializedProfile = await userProfileGrain.GetSerializedUserProfile();

            await this.HttpContext.Response.Body.WriteAsync(serializedProfile);
        }
        catch (Exception ex) {
            var commandType = command?.GetType().Name ?? "[NULL]";
            log.LogError(ex, "An exception occured during user profile request (UserId={UserId}, CommandType={CommandType})",
                userId, commandType);
            this.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}