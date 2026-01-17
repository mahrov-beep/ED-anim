namespace Multicast.Modules.AdAchievements.Adjust {
    using Cysharp.Threading.Tasks;
    using Multicast.Analytics;
    using Install;
    using Modules.Adjust;
    using UnityEngine;

    public class AdjustAdAchievementsAnalyticsModule : ScriptableModule {
        [SerializeField] private AdjustAdAchievementsConfiguration configuration;

        public override void Setup(ModuleSetup module) {
        }

        public override async UniTask Install(Resolver resolver) {
#if ADJUST_SDK
            await resolver.Get<SdkInitializationMarkers.Adjust>();

            var analyticsRegistration = await resolver.Get<IAnalyticsRegistration>();

            analyticsRegistration.RegisterAdapter(new AdjustAdAchievementsAnalyticsAdapter(this.configuration));
#else
            Debug.LogError($"Project does not contains ADJUST_SDK define. Add it or remove {this.name}");
#endif
        }
    }
}