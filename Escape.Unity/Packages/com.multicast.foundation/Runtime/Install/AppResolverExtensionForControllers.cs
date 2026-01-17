namespace Multicast.Install {
    using System.Reflection;
    using Cysharp.Threading.Tasks;

    public static class AppResolverExtensionForControllers {
        public static async UniTask BindAllControllersInAssembly(this ScriptableModule.Resolver resolver, Assembly assembly) {
            var baseControllerType = typeof(IControllerBase);
            var openControllerType = typeof(IControllerBase<>);

            foreach (var type in assembly.GetTypes()) {
                if (!type.IsClass || type.IsAbstract || !baseControllerType.IsAssignableFrom(type)) {
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

                    if (geneticTypeDefinition == openControllerType) {
                        var commandType = typeInterface.GetGenericArguments()[0];

                        var factory = await resolver.CreateFactoryInternalAsync<IControllerBase>(type);
                        ControllersShared.RegisterControllerFactory(commandType, factory);
                    }
                }
            }
        }
    }
}