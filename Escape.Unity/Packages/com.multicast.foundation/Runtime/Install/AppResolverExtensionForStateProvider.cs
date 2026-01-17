namespace Multicast.Install {
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using UniMob.UI;

    public static class AppResolverExtensionForStateProvider {
        public static async UniTask BindAllWidgetStatesInAssembly(this ScriptableModule.Resolver resolver, Assembly assembly, StateProvider stateProvider) {
            var baseStateType     = typeof(IState);
            var openViewStateType = typeof(ViewState<>);
            var openHocStateType  = typeof(HocState<>);

            foreach (var type in assembly.GetTypes()) {
                if (!type.IsClass || type.IsAbstract || !baseStateType.IsAssignableFrom(type)) {
                    continue;
                }

                if (AppResolverExtensions.ShouldSkip(resolver, type)) {
                    continue;
                }

                for (var currentType = type; currentType != null; currentType = currentType.BaseType) {
                    if (!currentType.IsGenericType) {
                        continue;
                    }

                    var genericTypeDefinition = currentType.GetGenericTypeDefinition();
                    if (genericTypeDefinition != openViewStateType &&
                        genericTypeDefinition != openHocStateType) {
                        continue;
                    }

                    var widgetType = currentType.GetGenericArguments()[0];
                    var factory    = await resolver.CreateFactoryInternalAsync<State>(type);

                    stateProvider.RegisterUnsafe(widgetType, factory);
                }
            }
        }
    }
}