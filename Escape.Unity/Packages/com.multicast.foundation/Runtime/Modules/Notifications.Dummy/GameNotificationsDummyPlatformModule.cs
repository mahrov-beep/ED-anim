namespace Multicast.Modules.Notifications.Dummy {
    using Cysharp.Threading.Tasks;
    using Install;
    using Multicast.Notifications;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class GameNotificationsDummyPlatformModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
            module.Provides<IGameNotificationsPlatformFactory>();
        }

        public override async UniTask Install(Resolver resolver) {
            resolver.Register<IGameNotificationsPlatformFactory>().To(new DummyNotificationPlatformFactory());
        }
    }
}