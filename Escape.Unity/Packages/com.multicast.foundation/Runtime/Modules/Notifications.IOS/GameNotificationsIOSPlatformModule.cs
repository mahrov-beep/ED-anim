namespace Multicast.Modules.Notifications.IOS {
    using Cysharp.Threading.Tasks;
    using Multicast.Install;
    using Multicast.Notifications;
    using UnityEngine;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class GameNotificationsIOSPlatformModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
#if UNITY_MOBILE_NOTIFICATIONS && UNITY_IOS
            module.Provides<IGameNotificationsPlatformFactory>();
#endif
        }

        public override async UniTask Install(Resolver resolver) {
#if UNITY_MOBILE_NOTIFICATIONS && UNITY_IOS
            resolver.Register<IGameNotificationsPlatformFactory>().To(new IOSNotificationPlatformFactory());
#else
            Debug.LogError($"Project does not contains  UNITY_MOBILE_NOTIFICATIONS && UNITY_IOS define. Add it or remove {this.name}");
#endif
        }
    }
}