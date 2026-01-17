namespace Game.ECS {
    using Quantum;
    using Scellecs.Morpeh;
    using Systems;
    using Systems.Core;
    using Systems.GameModels;
    using Systems.GameInventory;
    using Systems.Player;

    public class MainMenuGameResultsAdditiveInstaller : MonoInstallerBase {
        public override void Install(SystemsGroup systems) {
            systems.AddExistingSystem<LifetimeSystem>();
            systems.AddExistingSystem<LocalPlayerSystem>();
            systems.AddExistingSystem<QuantumEntityViewSystem>();

            systems.AddExistingSystem<UpdateGameLocalCharacterModelSystem>();
            systems.AddExistingSystem<GameInventorySystem>();
            systems.AddExistingSystem<GameInventoryWeaponSystem>();
        }
    }
}