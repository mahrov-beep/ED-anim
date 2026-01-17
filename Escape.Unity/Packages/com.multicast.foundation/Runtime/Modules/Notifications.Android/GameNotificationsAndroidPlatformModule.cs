namespace Multicast.Modules.Notifications.Android {
    using Cysharp.Threading.Tasks;
    using Multicast.Install;
    using Multicast.Notifications;
    using UnityEngine;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class GameNotificationsAndroidPlatformModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
#if UNITY_MOBILE_NOTIFICATIONS && UNITY_ANDROID
            module.Provides<IGameNotificationsPlatformFactory>();
#endif
        }

        public override async UniTask Install(Resolver resolver) {
#if UNITY_MOBILE_NOTIFICATIONS && UNITY_ANDROID
            resolver.Register<IGameNotificationsPlatformFactory>().To(new AndroidNotificationPlatformFactory());
#else
            Debug.LogError($"Project does not contains UNITY_MOBILE_NOTIFICATIONS && UNITY_ANDROID define. Add it or remove {this.name}");
#endif
        }
    }
}