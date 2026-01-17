namespace Game.ECS {
    using Initializers;
    using Quantum;
    using Scellecs.Morpeh;
    using Simulation.Systems.Units;
    using Systems;
    using Systems.Attack;
    using Systems.Camera;
    using Systems.Core;
    using Systems.FloatingText;
    using Systems.GameModels;
    using Systems.GameInventory;
    using Systems.Grenade;
    using Systems.Input;
    using Systems.Player;
    using Systems.Sounds;
    using Systems.Storage;
    using Systems.Unit;
    using Systems.WorldView;

    public class SimulationInstaller : MonoInstallerBase {
        public override void Install(SystemsGroup systems) {
            systems.AddExistingSystem<InputPollSystem>();
            systems.AddExistingSystem<AimingAssistSystem>();

            systems.AddExistingSystem<LifetimeSystem>();

            systems.AddExistingSystem<EventUnitDeadHandler>();
            systems.AddExistingSystem<EventUnitRebirthHandler>();

            systems.AddExistingSystem<LocalPlayerSystem>();
            systems.AddExistingSystem<QuantumEntityViewSystem>();

            systems.AddExistingSystem<MapperUnitEntityRefToEntitySystem>();
            systems.AddExistingSystem<MapperAimTargetGoToUnitEntity>();

            systems.AddExistingSystem<MarkLocalUnitSystem>();
            systems.AddExistingSystem<MarkEnemyUnitSystem>();

            systems.AddExistingSystem<UnitHealthBarMarkerSystem>();
            systems.AddExistingSystem<UnitCharacterVisualSystem>();
            systems.AddExistingSystem<PlacementPreviewViewSystem>();

            // camera
            systems.AddExistingSystem<CurrentCameraSystem>();
            systems.AddExistingSystem<CinemachineSetFollowSystem>();
            systems.AddExistingSystem<CinemachineOffsetSystem>();
            systems.AddExistingSystem<CinemachineBrainUpdateSystem>();
            systems.AddExistingSystem<MarkUnitsInLocalCameraFrustumSystem>();
            systems.AddExistingSystem<MarkUnitInFrustumAsVisiblySystem>();
            
            systems.AddExistingSystem<UpdateMapModelSystem>();

            systems.AddExistingSystem<QuantumPredictionCullingSystem>();
            systems.AddExistingSystem<UnitThirdPersonAimPositionPredictionSystem>();

            systems.AddExistingSystem<AttackVisualSystem>();
         
            systems.AddExistingSystem<AttackHitSoundSystem>();   
            systems.AddExistingSystem<DebuffSlowVisualSystem>();
            systems.AddExistingSystem<DebuffBurnVisualSystem>();

            //world view
            systems.AddExistingSystem<UnitHealthBarUiDynamicDataSystem>();
            systems.AddExistingSystem<DynamicAimUiDynamicDataSystem>();
            systems.AddExistingSystem<HitMarkUiDynamicDataSystem>();
            systems.AddExistingSystem<ItemBoxTimerUiDynamicDataSystem>();
            systems.AddExistingSystem<WorldViewUpdateSystem>();

            systems.AddExistingSystem<UpdateGameLocalCharacterModelSystem>();
            systems.AddExistingSystem<UpdateGameStateModelSystem>();
            systems.AddExistingSystem<GameInventorySystem>();
            systems.AddExistingSystem<GameInventoryWeaponSystem>();
            systems.AddExistingSystem<GameInventoryAbilitySystem>();
            systems.AddExistingSystem<ItemBoxStorageSystem>();
            systems.AddExistingSystem<GameNearbyItemSystem>();
            systems.AddExistingSystem<GameNearbyInteractiveZoneSystem>();
            // systems.AddExistingSystem<FloatingTextSystem>();
            systems.AddExistingSystem<ItemBoxOutlineSystem>();
            systems.AddExistingSystem<UISlowDebuffSystem>();
            systems.AddExistingSystem<StoredListenedStepsCueSystem>();
            systems.AddExistingSystem<StoredListenedShootCueSystem>();
            systems.AddExistingSystem<UpdateDamageCueModelSystem>();
            
            // Grenade indicators
            systems.AddExistingSystem<GrenadeTrackingSystem>();
            systems.AddExistingSystem<GrenadeIndicatorSystem>();
            systems.AddExistingSystem<ReconOutlineSystem>();

            systems.AddInitializer(new ItemBoxOpenCloseAnimationsInit());
            
            //sound effects
            systems.AddExistingSystem<BackgroundAudioSystem>();
            systems.AddExistingSystem<AudioListenerSystem>();
            
            //item box
            systems.AddExistingSystem<ItemBoxTimerSystem>();
        }
    }
}
