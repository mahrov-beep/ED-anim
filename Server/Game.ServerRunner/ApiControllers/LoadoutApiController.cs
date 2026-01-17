namespace Game.ServerRunner.ApiControllers;

using Grains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multicast;
using Shared;
using Shared.DTO;
using Shared.UserProfile.Data;
using Game.ServerRunner;

[ApiController]
[Produces(ServerConstants.CONTENT_TYPE_MSGPACK)]
public class LoadoutApiController(ILogger<LoadoutApiController> logger, IGrainFactory grainFactory) : Controller {
    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Loadout.GET_BY_USER)]
    public async Task<ServerResult<LoadoutGetByUserResponse>> GetByUser(LoadoutGetByUserRequest request) {
        if (request is null || request.UserId == Guid.Empty) {
            return ServerResult.Error(1, "Invalid request");
        }

        var userProfile = grainFactory.GetUserProfileGrain(request.UserId);
        var loadout = await userProfile.GetSelectedLoadoutSnapshot();
        return new LoadoutGetByUserResponse { Loadout = loadout };
    }
}


