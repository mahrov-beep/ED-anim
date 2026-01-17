namespace Multicast.Modules.UiDynamicContext {
    using Cysharp.Threading.Tasks;
    using Install;
    using UiDynamicContext = Multicast.UiDynamicContext;

    public class UiDynamicContextModule : IScriptableModule {
        public string name { get; } = "UiDynamicContext";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
            module.Provides<UiDynamicContext>();
        }

        public void PreInstall() {
        }

        public UniTask Install(ScriptableModule.Resolver resolver) {
            resolver.Register<UiDynamicContext>().To(new UiDynamicContext());
            return UniTask.CompletedTask;
        }

        public void PostInstall() {
        }
    }
}