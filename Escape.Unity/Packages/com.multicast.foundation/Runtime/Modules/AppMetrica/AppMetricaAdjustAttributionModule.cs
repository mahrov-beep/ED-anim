namespace Multicast.Modules.AppMetrica {
#if ADJUST_SDK
    using AdjustSdk;
#endif
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Install;
    using UnityEngine;

    public class AppMetricaAdjustAttributionModule : IScriptableModule, IScriptableModuleWithPriority {
#if ADJUST_SDK
        private static readonly UniTaskCompletionSource<AdjustAttribution> AttributionTcs = new();
#endif

        [RuntimeInitializeOnLoadMethod]
        private static void RegisterAttributionCallback() {
#if ADJUST_SDK && APPMETRICA_SDK
            Adjust.GetAttribution(HandleAttribution);

            static void HandleAttribution(AdjustAttribution it) {
                if (it == null) {
                    return;
                }

                AttributionTcs.TrySetResult(it);
            }
#endif
        }

        public string name => "Adjust.AppMetricaIntegration";

        public int Priority => ScriptableModulePriority.EARLY - 100;

        public bool IsPlatformSupported(string platform) {
#if ADJUST_SDK && APPMETRICA_SDK
            return true;
#else
            return false;
#endif
        }

        public void Setup(ScriptableModule.ModuleSetup module) {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
#if ADJUST_SDK && APPMETRICA_SDK
            if (!resolver.HasProviderFor(typeof(SdkInitializationMarkers.AppMetrica))) {
                Debug.LogWarning($"{this.name} installation skipped: AppMetrica module not exist");
                return;
            }

            if (!resolver.HasProviderFor(typeof(SdkInitializationMarkers.Adjust))) {
                Debug.LogWarning($"{this.name} installation skipped: Adjust module not exist");
                return;
            }

            await resolver.Get<SdkInitializationMarkers.AppMetrica>();
            await resolver.Get<SdkInitializationMarkers.Adjust>();

            this.AttributeInBackground().Forget();
#else
            Debug.LogWarning($"{this.name} installation skipped: Project does not contains ADJUST_SDK and APPMETRICA_SDK define. Add it or remove {this.name}");
#endif
        }

        public void PreInstall() {
        }

        public void PostInstall() {
        }

#if ADJUST_SDK && APPMETRICA_SDK
        private async UniTask AttributeInBackground() {
            var data = await AttributionTcs.Task;

            CoreAnalytics.ReportEvent("Adjust AttributionChangedDelegate", new Dictionary<string, object> {
                {"adgroup", data.Adgroup},
                {"campaign", data.Campaign},
                {"clickLabel", data.ClickLabel},
                {"costAmount", data.CostAmount.GetValueOrDefault(0.0)},
                {"costCurrency", data.CostCurrency},
                {"network", data.Network},
                {"trackerName", data.TrackerName},
                {"trackerToken", data.TrackerToken},
                {"creative", data.Creative},
            });

            this.ReportReferral(data);
        }

        public void ReportReferral(AdjustAttribution data) {
            var attr = Io.AppMetrica.ExternalAttributions.Adjust(data);
            Io.AppMetrica.AppMetrica.ReportExternalAttribution(attr);

            Debug.Log("AppMetrica.ReportExternalAttribution()");
        }

#endif
    }
}