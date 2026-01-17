namespace Multicast.Modules.Analytics {
    using Cysharp.Threading.Tasks;
    using Install;
    using Multicast.Analytics;

    internal sealed class AnalyticsModule : IScriptableModule {
        public string name { get; } = "Analytics";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
            module.Provides<IAnalytics>();
            module.Provides<IAnalyticsRegistration>();
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            var analytics = await resolver.Register<IAnalytics>().ToAsync<Analytics>();
            resolver.Register<IAnalyticsRegistration>().To(analytics);
        }

        public void PostInstall() {
        }
    }
}