namespace Game.ECS {
    using Scellecs.Morpeh;
    using Systems.Core;
    using Systems.GameModels;
    using Systems.GameInventory;
    using Systems.Player;
    using Systems.Storage;

    public class MainMenuStorageAdditiveInstaller : MonoInstallerBase {
        public override void Install(SystemsGroup systems) {
            systems.AddExistingSystem<LifetimeSystem>();
            systems.AddExistingSystem<LocalPlayerSystem>();
            systems.AddExistingSystem<QuantumEntityViewSystem>();

            systems.AddExistingSystem<UpdateGameLocalCharacterModelSystem>();
            systems.AddExistingSystem<GameInventorySystem>();
            systems.AddExistingSystem<GameInventoryWeaponSystem>();
            systems.AddExistingSystem<ItemBoxStorageSystem>();
            systems.AddExistingSystem<GameNearbyItemSystem>();
        }
    }
}