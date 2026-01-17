namespace Multicast.Modules.AdAchievements.AppMetrica {
    using Cysharp.Threading.Tasks;
    using Multicast.Analytics;
    using Install;
    using Modules.AppMetrica;
    using UnityEngine;

    public class AppMetricaAdAchievementsAnalyticsModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
        }

        public override async UniTask Install(Resolver resolver) {
#if APPMETRICA_SDK
            await resolver.Get<SdkInitializationMarkers.AppMetrica>();

            var analyticsRegistration = await resolver.Get<IAnalyticsRegistration>();

            analyticsRegistration.RegisterAdapter(new AppMetricaAdAchievementsAnalyticsAdapter());
#else
            Debug.LogError($"Project does not contains APPMETRICA_SDK define. Add it or remove {this.name}");
#endif
        }
    }
}