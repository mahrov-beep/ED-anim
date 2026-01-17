namespace Multicast.Modules.CommandHandlers {
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using Install;
    using UniMob.UI;

    public class HandlersModule : IScriptableModule,
        IScriptableModuleWithPriority {
        private readonly Assembly[]    assemblies;
        private readonly StateProvider stateProvider;

        public string name { get; } = "Handlers";

        public bool IsPlatformSupported(string platform) => true;

        public int Priority => ScriptableModulePriority.LATE;

        public HandlersModule(Assembly[] assemblies, StateProvider stateProvider) {
            this.assemblies    = assemblies;
            this.stateProvider = stateProvider;
        }

        public void Setup(ScriptableModule.ModuleSetup module) {
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            foreach (var assembly in this.assemblies) {
                await resolver.BindAllControllersInAssembly(assembly);
                await resolver.BindAllServerCommandHandlersInAssembly(assembly);
                await resolver.BindAllCommandHandlersInAssembly(assembly);
                await resolver.BindAllWidgetStatesInAssembly(assembly, this.stateProvider);
            }
        }

        public void PostInstall() {
        }
    }
}