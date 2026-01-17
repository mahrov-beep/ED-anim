namespace Multicast.Modules.CoreAchievements {
    using Cysharp.Threading.Tasks;
    using Multicast.CoreAchievements;
    using Multicast.Install;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class CoreAchievementsModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
            module.Provides<CoreAchievementsModel>();
        }

        public override async UniTask Install(Resolver resolver) {
            await resolver.Register<CoreAchievementsModel>().ToAsync<CoreAchievementsModel>();
        }
    }
}