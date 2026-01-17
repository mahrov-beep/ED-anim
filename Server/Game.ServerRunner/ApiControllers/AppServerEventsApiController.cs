namespace Game.ServerRunner.ApiControllers;

using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Streams;
using Shared;
using Shared.ServerEvents;
using Game.ServerRunner.Grains;

[ApiController]
public class AppServerEventsApiController(
    ILogger<AppServerEventsApiController> logger,
    IClusterClient clusterClient
) : Controller {
    [Authorize]
    [HttpGet(SharedConstants.UrlRoutes.ServerEvents.APP)]
    public Task AppEvents() {
        return ServerEvents<IAppServerEvent>.AcceptHttpContextAsync(logger, this.HttpContext, async (userId, send, close) => {
            var status = clusterClient.GetGrain<IUserStatusGrain>(userId);
            await status.ClientConnected();

            var subscriptionHandle = await clusterClient
                .GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
                .GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(userId))
                .SubscribeAsync(async (data, _) => await send(data));

            return async () => {
                await subscriptionHandle.UnsubscribeAsync();
                await status.ClientDisconnected();
            };
        });
    }
}