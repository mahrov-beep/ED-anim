namespace Game.ECS {
    using Scellecs.Morpeh;
    using Systems.Sounds;
    using Systems.WorldView;

    public class MainMenuInstaller : MonoInstallerBase {
        public override void Install(SystemsGroup systems) {
            systems.AddExistingSystem<WorldViewUpdateSystem>();
            systems.AddExistingSystem<UnitPartyUiDynamicDataSystem>();
            
            systems.AddExistingSystem<BackgroundAudioSystem>();
        }
    }
}

