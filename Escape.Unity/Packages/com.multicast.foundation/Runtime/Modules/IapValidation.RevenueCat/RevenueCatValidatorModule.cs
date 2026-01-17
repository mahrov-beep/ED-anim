namespace Multicast.Modules.IapValidation.RevenueCat {
#if REVENUE_CAT_SDK && ADJUST_SDK
    using AdjustSdk;
#endif
    using Cysharp.Threading.Tasks;
    using Install;
    using Morpeh;
    using Purchasing.UnityIAP;
    using UnityEngine;
    using Multicast.Analytics;
    using Multicast.Purchasing;
    using Scellecs.Morpeh;
    using UserTracking;

#if !REVENUE_CAT_SDK
    [Sirenix.OdinInspector.TypeInfoBox("Missing define REVENUE_CAT_SDK")]
#endif
    public class RevenueCatValidatorModule : ScriptableModule {
#if REVENUE_CAT_SDK
        private static MulticastRevenueCatListener Listener;
#endif

        [RuntimeInitializeOnLoadMethod]
        private static void RegisterAttributionCallback() {
#if REVENUE_CAT_SDK && ADJUST_SDK
            Adjust.GetAttribution(HandleAttribution);

            static void HandleAttribution(AdjustAttribution it) {
                if (it == null) {
                    return;
                }

                if (Listener == null || Listener.Purchases == null) {
                    return;
                }

                SyncAdjustId();
            }
#endif
        }

        public override void Setup(ModuleSetup module) {
#if REVENUE_CAT_SDK
            module.Provides<SdkInitializationMarkers.RevenueCat>();
#endif
        }

        public override async UniTask Install(Resolver resolver) {
#if REVENUE_CAT_SDK
            resolver.Register<SdkInitializationMarkers.RevenueCat>().To(new SdkInitializationMarkers.RevenueCat());

            var registration   = await resolver.Get<IUnityIapValidationsRegistration>();
            var analytics      = await resolver.Get<IAnalytics>();
            var purchasing     = await resolver.Get<IPurchasing>();
            var validationRepo = await resolver.Get<UdRevenueCatValidationRepo>();
            var worldReg       = await resolver.Get<IWorldRegistration>();

            await resolver.Get<IUserTrackingService>();

            var revenueCatPurchases = GameObject.FindObjectOfType<Purchases>();

            if (revenueCatPurchases == null) {
                Debug.LogError("No Purchases object found in scene. Please check Revenue Cat SDK integration");
            }

            Listener = revenueCatPurchases.GetComponent<MulticastRevenueCatListener>();

            if (Listener == null) {
                Debug.LogError($"No MulticastRevenueCatListener script found on {revenueCatPurchases.name}. Please check Revenue Cat SDK integration");
                return;
            }

            var validator = new RevenueCatValidator(Listener);
            registration.RegisterValidator(validator);

            Listener.Initialize(analytics, validationRepo, purchasing);

            worldReg.RegisterInstaller(s => this.InstallSystems(s));
#else
            Debug.LogError($"Project does not contains REVENUE_CAT_SDK define. Add it or remove {this.name}");
#endif

            SyncAdjustId();
        }

        private static void SyncAdjustId() {
#if REVENUE_CAT_SDK && ADJUST_SDK
            Listener.Purchases.CollectDeviceIdentifiers();
            Adjust.GetAdid(adId => {
                if (adId != null) {
                    Listener.Purchases.SetAdjustID(adId);
                }
            });
#endif
        }

        private void InstallSystems(SystemsGroup systems) {
#if REVENUE_CAT_SDK
            systems.AddExistingSystem<RevenueCatValidationSystem>();

            World.Default.GetExistingSystem<RevenueCatValidationSystem>().Listener = Listener;
#endif
        }
    }
}