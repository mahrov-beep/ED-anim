namespace Game.ServerRunner.ApiControllers;

using Core;
using Microsoft.AspNetCore.Mvc;
using Orleans.Streams;
using Shared;
using Shared.ServerEvents;

[ApiController]
public class GameServerEventsApiController(
    ILogger<GameServerEventsApiController> logger,
    IClusterClient clusterClient
) : Controller {
    [HttpGet(SharedConstants.UrlRoutes.ServerEvents.GAME)]
    public Task GameEvents([FromQuery] string gameId) {
        if (string.IsNullOrEmpty(gameId)) {
            this.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Task.CompletedTask;
        }

        return ServerEvents<IGameServerEvent>.AcceptHttpContextAsync(logger, this.HttpContext, async (userId, send, close) => {
            // TODO verify gameId

            var subscriptionHandle = await clusterClient
                .GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
                .GetStream<IGameServerEvent>(OrleansConstants.Streams.Ids.GameServerEventForGame(gameId))
                .SubscribeAsync(async (data, _) => await send(data));

            return async () => await subscriptionHandle.UnsubscribeAsync();
        });
    }
}