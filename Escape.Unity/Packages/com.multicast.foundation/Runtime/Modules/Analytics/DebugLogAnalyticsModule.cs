namespace Multicast.Modules.Analytics {
    using Cysharp.Threading.Tasks;
    using GameProperties;
    using GreenButtonGames.Analytics.DebugLog;
    using Install;
    using Multicast.Analytics;

    public class DebugLogAnalyticsModule : IScriptableModule {
        public string name => "Debug Log Analytics";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            var analyticsRegistration = await resolver.Get<IAnalyticsRegistration>();
            var properties            = await resolver.Get<GamePropertiesModel>();

            analyticsRegistration.RegisterAdapter(new DebugLogAnalyticsAdapter(properties));
        }

        public void PostInstall() {
        }
    }
}