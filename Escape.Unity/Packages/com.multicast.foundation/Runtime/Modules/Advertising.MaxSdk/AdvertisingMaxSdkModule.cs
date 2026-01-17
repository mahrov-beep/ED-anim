namespace Multicast.Modules.Advertising.MaxSdk {
    using System;
    using Cheats;
    using Cysharp.Threading.Tasks;
    using FeatureToggles;
    using Multicast.Advertising;
    using Multicast.Install;
    using UnityEngine;
    using UserTracking;

    public class AdvertisingMaxSdkModule : ScriptableModule {
        [SerializeField] private MaxSdkAdConfiguration adConfiguration;

        public override void Setup(ModuleSetup module) {
#if APPLOVIN_MAX_SDK
            module.Provides<IAdvertising>();
#endif
        }

        public override async UniTask Install(Resolver resolver) {
#if APPLOVIN_MAX_SDK
            var trackingService = await resolver.Get<IUserTrackingService>();
            var cheatButtons    = await resolver.Get<ICheatButtonsRegistry>();
            var cheatProperties = await resolver.Get<ICheatGamePropertiesRegistry>();
            var features        = await resolver.Get<Features>();

            cheatProperties.Register(AdGameProperties.AdNoRewarded);
            cheatProperties.Register(MaxSdkGameProperties.RewardedAlwaysAvailable);

            var adConfig = this.adConfiguration;

            SetupRotatedAdUnitTest(ref adConfig, features, MaxSdkFeatureToggles.RotatedRewardedAdUnitsAndroid);
            SetupRotatedAdUnitTest(ref adConfig, features, MaxSdkFeatureToggles.RotatedRewardedAdUnitsIOS);

            var sdk = await resolver.Register<IAdvertising>().ToAsync<AdvertisingApplovinMaxSdk, MaxSdkAdConfiguration>(adConfig);

            sdk.InitializeAsync().Forget();

            cheatButtons.RegisterAction("MaxSdk - Open Ad Debugger", () => sdk.ShowDebugger());
#else
            Debug.LogError($"Project does not contains APPLOVIN_MAX_SDK define. Add it or remove {this.name}");
#endif
        }

        private static void SetupRotatedAdUnitTest(ref MaxSdkAdConfiguration adConfig, FeatureTogglesModel features, FeatureToggleName featureToggleName) {
            if (!features.IsFeatureExist(featureToggleName)) {
                return;
            }

            var adUnitOverrideStr = features.GetText(featureToggleName);
            var adUnitOverrideIds = adUnitOverrideStr.Split('|', StringSplitOptions.RemoveEmptyEntries);
            adUnitOverrideIds = Array.ConvertAll(adUnitOverrideIds, it => it.Trim());

            if (adUnitOverrideIds.Length <= 0) {
                return;
            }

            adConfig = adConfig.WithRewardedAdUnitOverride(new MaxSdkRotatedAdUnitOverride(adUnitOverrideIds));

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"MaxSdk - Register RotatedAdUnit override: {adUnitOverrideStr}");
            }
        }
    }
}