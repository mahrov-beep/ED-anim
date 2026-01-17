namespace Game.ServerRunner.ApiControllers;

using Grains;
using Microsoft.AspNetCore.Mvc;
using Multicast;
using Shared;
using Shared.DTO;

[ApiController]
[Produces(ServerConstants.CONTENT_TYPE_MSGPACK)]
public class AuthApiController(
    IGrainFactory grainFactory
) : Controller {
    [HttpPost(SharedConstants.UrlRoutes.Auth.GUEST)]
    public async Task<ServerResult<GuestAuthResponse>> AuthGuest(GuestAuthRequest request) {
        return await grainFactory.GetAuthGrain().AuthGuest(request);
    }
}