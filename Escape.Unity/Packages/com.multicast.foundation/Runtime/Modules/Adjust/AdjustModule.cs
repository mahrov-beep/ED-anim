namespace Multicast.Modules.Adjust {
#if ADJUST_SDK
    using AdjustSdk;
#endif
    using Cysharp.Threading.Tasks;
    using Install;
    using UnityEngine;
    using UserTracking;

    public class AdjustModule : ScriptableModule, IScriptableModuleWithPriority {
        public int Priority { get; } = ScriptableModulePriority.EARLY;

        public override void Setup(ModuleSetup module) {
#if ADJUST_SDK
            module.Provides<SdkInitializationMarkers.Adjust>();
#endif
        }

        public override async UniTask Install(Resolver resolver) {
#if ADJUST_SDK
            await resolver.Get<IUserTrackingService>();

            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                Debug.Log("Adjust.checkForNewAttStatus()");
                Adjust.GetAppTrackingAuthorizationStatus(); //Adjust.CheckForNewAttStatus();
            }

            resolver.Register<SdkInitializationMarkers.Adjust>().To(new SdkInitializationMarkers.Adjust());
#else
            Debug.LogError($"Project does not contains ADJUST_SDK define. Add it or remove {this.name}");
#endif
        }
    }
}