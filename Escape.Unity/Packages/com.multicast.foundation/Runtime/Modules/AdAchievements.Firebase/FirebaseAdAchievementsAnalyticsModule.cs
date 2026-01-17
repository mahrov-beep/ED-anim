namespace Multicast.Modules.AdAchievements.Firebase {
    using Cysharp.Threading.Tasks;
    using Multicast.Analytics;
    using Install;
    using Modules.Firebase;
    using UnityEngine;

    public class FirebaseAdAchievementsAnalyticsModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
        }

        public override async UniTask Install(Resolver resolver) {
#if FIREBASE_SDK
            await resolver.Get<SdkInitializationMarkers.Firebase>();

            var analyticsRegistration = await resolver.Get<IAnalyticsRegistration>();

            analyticsRegistration.RegisterAdapter(new FirebaseAdAchievementsAnalyticsAdapter());
#else
            Debug.LogError($"Project does not contains FIREBASE_SDK define. Add it or remove {this.name}");
#endif
        }
    }
}