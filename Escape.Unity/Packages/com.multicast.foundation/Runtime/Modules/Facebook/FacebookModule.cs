namespace Multicast.Modules.Facebook {
    using Cysharp.Threading.Tasks;
#if FACEBOOK_SDK
    using global::Facebook.Unity;
#endif
    using Install;
    using UnityEngine;
    using UserTracking;

#if !FACEBOOK_SDK
    [Sirenix.OdinInspector.TypeInfoBox("Missing define FACEBOOK_SDK")]
#endif
    public class FacebookModule : ScriptableModule, IScriptableModuleWithPriority {
        public int Priority { get; } = ScriptableModulePriority.EARLY;

        public override void Setup(ModuleSetup module) {
        }

        public override async UniTask Install(Resolver resolver) {
#if FACEBOOK_SDK
            await resolver.Get<IUserTrackingService>();

            AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);

            await FacebookEvents.ActivateAsync();

            if (!Application.isEditor) {
                FB.Mobile.SetAdvertiserTrackingEnabled(true);
                FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
            }

            Debug.Log($"FB.Mobile.SetAdvertiserTrackingEnabled()");
#else
            Debug.LogError($"Project does not contains FACEBOOK_SDK define. Add it or remove {this.name}");
#endif
        }
    }
}