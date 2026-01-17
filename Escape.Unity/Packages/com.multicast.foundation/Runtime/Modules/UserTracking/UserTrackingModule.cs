namespace Multicast.Modules.UserTracking {
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Install;
    using UnityEngine;

    public class UserTrackingModule : IScriptableModule, IScriptableModuleWithPriority {
        public string name => "UserTracking";

        public bool IsPlatformSupported(string platform) => true;

        public int Priority => ScriptableModulePriority.EARLY - 100;

        public void Setup(ScriptableModule.ModuleSetup module) {
            module.Provides<IUserTrackingService>();
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            switch (Application.platform) {
                case RuntimePlatform.IPhonePlayer:
                    await this.InstallApple(resolver);
                    break;

                default:
                    await this.InstallDefault(resolver);
                    break;
            }

            Debug.Log("UserTracking status was checked");
        }

        public void PreInstall() {
        }

        public void PostInstall() {
#if ADJUST_SDK
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                var status = (ATTrackingAuthStatus) AdjustSdk.Adjust.GetAppTrackingAuthorizationStatus();

                Debug.Log($"ATT status = {status}");

                CoreAnalytics.ReportEvent("att_tracking", new Dictionary<string, object> {
                    ["status"] = status.ToString(),
                });
            }
#endif
        }

        private async UniTask InstallApple(ScriptableModule.Resolver resolver) {
#if ADJUST_SDK
            var trackingTcs = new UniTaskCompletionSource();
            AdjustSdk.Adjust.RequestAppTrackingAuthorization(it => trackingTcs.TrySetResult());
            await trackingTcs.Task;

            resolver.Register<IUserTrackingService>().To(new UserTrackingService());
#else
            Debug.LogWarning("UserTracking not authorized, reason: Adjust SDK not installed");

            resolver.Register<IUserTrackingService>().To(new UserTrackingService());
#endif
        }

        private async UniTask InstallDefault(ScriptableModule.Resolver resolver) {
            resolver.Register<IUserTrackingService>().To(new UserTrackingService());
        }

        private class UserTrackingService : IUserTrackingService {
        }

        private enum ATTrackingAuthStatus {
            NotDetermined = 0,
            Unknown       = -1,
            Restricted    = 1,
            Denied        = 2,
            Authorized    = 3,
        }
    }
}