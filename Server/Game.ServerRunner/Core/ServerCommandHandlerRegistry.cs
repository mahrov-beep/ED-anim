namespace Game.ServerRunner.Core;

using System.Collections.Concurrent;
using Multicast;
using Multicast.ServerData;

public class ServerCommandHandlerRegistry<TServerDataContext, TServerData>(IServiceProvider serviceProvider)
    where TServerDataContext : class, IServerCommandContext
    where TServerData : SdObject {
    private readonly ConcurrentDictionary<Type, IServerCommandHandler<TServerDataContext, TServerData>> commandToHandler = new();

    public IServerCommandHandler<TServerDataContext, TServerData> GetHandler(Type commandType) {
        return this.commandToHandler.GetOrAdd(commandType, HandlerFactory, serviceProvider);

        static IServerCommandHandler<TServerDataContext, TServerData> HandlerFactory(Type commandType, IServiceProvider serviceProvider) {
            var handlerType = typeof(IServerCommandHandler<,,>).MakeGenericType(typeof(TServerDataContext), typeof(TServerData), commandType);
            var handler     = serviceProvider.GetRequiredService(handlerType);
            return (IServerCommandHandler<TServerDataContext, TServerData>)handler;
        }
    }
}