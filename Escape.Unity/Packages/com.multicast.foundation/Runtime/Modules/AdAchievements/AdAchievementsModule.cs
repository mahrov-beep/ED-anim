namespace Multicast.Modules.AdAchievements {
    using Cysharp.Threading.Tasks;
    using Install;
    using Morpeh;
    using Scellecs.Morpeh;

    public class AdAchievementsModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
            module.ProvidesKeyedModel<AdAchievementDef, AdAchievementData, AdAchievementModel, AdAchievementsModel>();
        }

        public override async UniTask Install(Resolver resolver) {
            await resolver.RegisterKeyedModel<AdAchievementDef, AdAchievementData, AdAchievementModel, AdAchievementsModel>();

            var worldReg = await resolver.Get<IWorldRegistration>();

            worldReg.RegisterInstaller(this.InstallSystem);
        }

        private void InstallSystem(SystemsGroup systems) {
            systems.AddExistingSystem<AdAchievementSystem>();
        }
    }
}