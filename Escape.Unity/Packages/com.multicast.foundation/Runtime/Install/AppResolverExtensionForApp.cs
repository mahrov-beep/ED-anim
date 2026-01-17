namespace Multicast.Install {
    using System.Reflection;
    using Cysharp.Threading.Tasks;

    public static class AppResolverExtensionForApp {
        public static async UniTask BindAllServerCommandHandlersInAssembly(this ScriptableModule.Resolver resolver, Assembly assembly) {
            var baseCommandHandlerType  = typeof(IServerCommandHandlerBase);
            var openCommandHandlerType1 = typeof(IServerCommandHandler<,,>);

            foreach (var type in assembly.GetTypes()) {
                if (!type.IsClass || type.IsAbstract || !baseCommandHandlerType.IsAssignableFrom(type)) {
                    continue;
                }

                foreach (var typeInterface in type.GetInterfaces()) {
                    if (!typeInterface.IsGenericType) {
                        continue;
                    }

                    var geneticTypeDefinition = typeInterface.GetGenericTypeDefinition();

                    if (geneticTypeDefinition == openCommandHandlerType1) {
                        var commandContextType = typeInterface.GetGenericArguments()[0];
                        var commandDataType    = typeInterface.GetGenericArguments()[1];
                        var commandType        = typeInterface.GetGenericArguments()[2];

                        var instance = (IServerCommandHandlerBase) await resolver.CreateInstanceInternalAsync(type);
                        App.Current.RegisterServerCommandHandler(commandContextType, commandDataType, commandType, instance);
                    }
                }
            }
        }

        public static async UniTask BindAllCommandHandlersInAssembly(this ScriptableModule.Resolver resolver, Assembly assembly) {
            var baseCommandHandlerType  = typeof(ICommandHandlerBase);
            var openCommandHandlerType1 = typeof(ICommandHandler<>);
            var openCommandHandlerType2 = typeof(ICommandHandler<,>);

            foreach (var type in assembly.GetTypes()) {
                if (!type.IsClass || type.IsAbstract || !baseCommandHandlerType.IsAssignableFrom(type)) {
                    continue;
                }

                if (AppResolverExtensions.ShouldSkip(resolver, type)) {
                    continue;
                }

                foreach (var typeInterface in type.GetInterfaces()) {
                    if (!typeInterface.IsGenericType) {
                        continue;
                    }

                    var geneticTypeDefinition = typeInterface.GetGenericTypeDefinition();

                    if (geneticTypeDefinition == openCommandHandlerType1) {
                        var commandType = typeInterface.GetGenericArguments()[0];

                        var instance = (ICommandHandlerBase) await resolver.CreateInstanceInternalAsync(type);
                        App.Current.RegisterCommandHandler(commandType, instance);
                    }
                    else if (geneticTypeDefinition == openCommandHandlerType2) {
                        var commandType = typeInterface.GetGenericArguments()[0];
                        var resultType  = typeInterface.GetGenericArguments()[1];

                        var instance = (ICommandHandlerBase) await resolver.CreateInstanceInternalAsync(type);
                        App.Current.RegisterCommandHandler(commandType, resultType, instance);
                    }
                }
            }
        }
    }
}