namespace Quantum {
  using System.Collections.Generic;
  using System.Diagnostics;
  using Bots;
  using BotSDK;
  using Core;
  using ItemBoxes;
  using Quests;

  public static unsafe partial class DeterministicSystemSetup {
    static partial void AddSystemsUser(ICollection<SystemBase> systems, RuntimeConfig gameConfig, SimulationConfig simulationConfig,
            SystemsConfig systemsConfig) {
      if (systemsConfig.systemsConfigType == SystemsConfigTypes.MainMenuGameResults) {
        AddMainMenuGameResultsSystems(systems, gameConfig, simulationConfig);
        return;
      }

      if (systemsConfig.systemsConfigType == SystemsConfigTypes.MainMenuStorage) {
        AddMainMenuStorageSystems(systems, gameConfig, simulationConfig);
        return;
      }

      AddGameplaySystems(systems, gameConfig, simulationConfig);
      AddDebugSystems(systems, gameConfig, simulationConfig);

      SystemsOrder.Validate(systems);
    }

    static void AddMainMenuGameResultsSystems(ICollection<SystemBase> systems, RuntimeConfig gameConfig,
            SimulationConfig simulationConfig) {
      AddQuantumCoreSystems(systems);

      systems.AddSystem<RandomShuffleSpawnPoints>();
      systems.AddSystem<UnitStatsSystem>();
      systems.AddSystem<WeaponStatsSystem>();
      systems.Add(CreateCharacterControlSystemGroup());
      systems.AddSystem<MainMenuSpawnCharacterSystem>();
    }

    static void AddMainMenuStorageSystems(ICollection<SystemBase> systems, RuntimeConfig gameConfig, SimulationConfig simulationConfig) {
      AddQuantumCoreSystems(systems);

      systems.AddSystem<RandomShuffleSpawnPoints>();
      systems.AddSystem<CommandsSystem>();
      systems.AddSystem<UnitStatsSystem>();
      systems.AddSystem<SafeInitSystem>();
      systems.AddSystem<WeaponStatsSystem>();
      systems.Add(CreateCharacterControlSystemGroup());
      systems.AddSystem<MainMenuSpawnCharacterSystem>();
      systems.AddSystem<CharacterLoadoutModifiedEventSystem>();
      systems.AddSystem<TriggerAreaDetectSystem>();
      systems.AddSystem<ItemBoxClearUnitSystem>();
      systems.AddSystem<ItemBoxNearbySystem>();
      systems.AddSystem<ItemBoxDestroyEmptySystem>();
    }

    static void AddGameplaySystems(ICollection<SystemBase> systems, RuntimeConfig gameConfig, SimulationConfig simulationConfig) {
      systems.AddSystem<LagCompensationSystem>(); // must be before Physics3DSystem

      AddQuantumCoreSystems(systems);
      
      systems.AddSystem<RandomShuffleSpawnPoints>();

      systems.AddComponentRemoveHandler(CharacterStateDead.EnableComponents);

      systems.AddComponentAddHandler(CharacterStateDead.DisableComponents);
      systems.AddComponentAddHandler(CharacterStateDead.ResetInput);
      systems.AddComponentAddHandler(CharacterStateDead.DebotifyDeadUnit);

      systems.AddComponentAddHandler(Unit.InitRNG);
      systems.AddComponentAddHandler(Unit.PrepareSlowDebuff);

      systems.AddSystem<GameStateSystem>();
      systems.AddSystem<EndGameOnDeathInEscapeModeSystem>();

      systems.AddSystem<CommandsSystem>();
      systems.AddSystem<ReadPlayerInputSystem>();
      systems.AddSystem<SetForceMoveForwardInputOverrideSystem>();

      systems.AddSystem<TurretSystem>();
      systems.AddComponentAddHandler(Turret.InitTriggerArea);

      systems.AddSystem<MineSystem>();
      systems.AddComponentAddHandler(Mine.InitTriggerArea);

      systems.AddSystem<PlayerJoiningSystem>();
      systems.AddSystem<CharacterConfigureUnitOnLoadoutModifications>();
      systems.AddSystem<CharacterAutoRefillAmmoSystem>();
      systems.AddSystem<CharacterThrowUsedItemsFromLoadoutSystem>();
      systems.AddSystem<CharacterLoadoutModifiedEventSystem>();
      systems.AddSystem<CharacterLoadoutDestroyItemsSystem>();

      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateIdle>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateWalk>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateKnocked>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateRoll>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateSprint>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateReviving>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateCrouchIdle>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateCrouchMove>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateHealing>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateKnifeAttack>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateDead>>();
      systems.AddSystem<CharacterStateUpdateSystem<CharacterStateJump>>();

      systems.AddSystem<ItemBoxClearUnitSystem>();
      systems.AddSystem<ItemBoxNearbySystem>();
      systems.AddSystem<ItemBoxSpawnPointSystem>();
      systems.AddSystem<ItemBoxCloseSystem>();
      systems.AddSystem<ItemBoxTimerSystem>();
      // systems.AddSystem<ItemBoxDestroyEmptySystem>();

      systems.AddSystem<UnitStatsSystem>();
      systems.AddSystem<WeaponStatsSystem>();
      systems.AddSystem<UnitKnockSystem>();
      systems.AddSystem<SlowStackableDebuffSystem>();
      // systems.AddSystem<BurnDebuffSystem>();

      systems.AddSystem<AbilitySystem>();
      systems.AddSystem<FlashbangBlindSystem>();
      systems.AddSystem<ReconEffectSystem>();

      systems.AddSystem<QuestCounterTaskSystem>();

      systems.AddSystem<BotMarkInvisibleSystem>();

      var aiSystemGroup = new AISystemGroup(children: new[] {
          CreateSystem<AILifecycleComponentsSignals>(),
          CreateSystem<AIRoomFillPlayerBotsSystem>(),
          CreateSystem<AIEnemiesNPCCreationSystem>(),

          CreateSystem<BotSDKTimerSystem>(),
          CreateSystem<AINavMeshSetDesiredMovementInput>(),
          CreateSystem<BotMovementInputSystem>(),
          CreateSystem<BotCameraRotationSystem>(),

          CreateSystem<PerceptionVisionSystem>(),
          CreateSystem<PerceptionHearingSystem>(),
          CreateSystem<PerceptionDamageSystem>(),
          CreateSystem<PerceptionMemorySystem>(),

          CreateSystem<BotBehaviourTreeUpdateSystem>(),
        }
      );
      systems.Add(aiSystemGroup);

      systems.Add(CreateCharacterControlSystemGroup());

      var gameplaySystemGroup = new GameplaySystemGroup(new[] {
              CreateSystem<HealthAttributesSystem>(),
              CreateSystem<AttributesSystem>(),

              CreateSystem<UnitAimingFlagSystem>(),
              CreateSystem<UnitFeatureSprintWithStaminaSystem>(),
              CreateSystem<AnimationSystem>(),
              CreateSystem<UnitAimSystem>(),
              CreateSystem<UnitFragsSystem>(),
              CreateComponentAddHandler(Attack.OnCreate),
              CreateSystem<WeaponReloadSystem>(),
              CreateSystem<WeaponChangeSystem>(),
              CreateSystem<WeaponCalcShootingSpreadSystem>(),
              CreateSystem<WeaponAimHoldTargetTimeSystem>(),
              CreateSystem<WeaponShootSystem>(),
              CreateSystem<WeaponRecoilSystem>(),

              CreateSystem<AttackSystem>(),
              CreateSystem<PersistentAoEExitSystem>(),

              CreateSystem<TriggerAreaDetectSystem>(),
              CreateSystem<InteractiveZoneSystem>(),
              CreateSystem<InteractiveZoneUnitClearSystem>(),
              CreateSystem<InteractiveZoneUnitNearbySystem>(),

              CreateSystem<UnitFeatureHealSelfOnDeathSystem>(),
              CreateSystem<UnitFeatureDropLoadoutOnDeathSystem>(),
              CreateSystem<MinimapPathToExitSystem>()
      });
      systems.Add(gameplaySystemGroup);

      systems.AddSystem<ObjectsLifetimeControlSystem>();
      systems.AddSystem<EntityChildSyncExistSystem>();
      systems.AddSystem<EntityChildSyncTransform3DSystem>();

      systems.AddSystem<DestroyDeadUnitSystem>();

      systems.AddSystem<LateLagCompensationSystem>();
    }

    static CharacterControlSystemGroup CreateCharacterControlSystemGroup() {
      return new CharacterControlSystemGroup(new[] {
        CreateSystem<UnitMovementSystem>(),
        CreateComponentAddHandler(CharacterSpectatorCamera.OnAdd),
        CreateComponentRemoveHandler(CharacterSpectatorCamera.OnRemove),
        CreateComponentAddHandler(UnitAim.OnAdd),
        CreateComponentRemoveHandler(UnitAim.OnRemove),
        CreateSystem<UnitRotationSystem>(),
        CreateSystem<RotateKCCToSpectatorRotationSystem>(),
        CreateSystem<KCCSystem>(),
        CreateSystem<UnitApplyTransformSystem>(),
        CreateSystem<UnitApplySpectatorCameraTransformSystem>(),
      });
    }

    static void AddQuantumCoreSystems(ICollection<SystemBase> systems) {
      systems.AddSystem<CullingSystem3D>();
      systems.AddSystem<PhysicsSystem3D>();
      systems.AddSystem<NavigationSystem>();
      systems.AddSystem<EntityPrototypeSystem>();
      systems.AddSystem<PlayerConnectedSystem>();
    }

    [Conditional("UNITY_EDITOR")]
    static void AddDebugSystems(ICollection<SystemBase> systems, RuntimeConfig gameConfig, SimulationConfig simulationConfig) {
      systems.AddSystem<DebugDrawSystem>();
      systems.AddSystem<AIDebugDrawSystem>();
      systems.AddSystem<BotSDKDebuggerSystem>();
      systems.AddSystem<PingPongTargetSystem>();
    }

    static SystemBase CreateSystem<T>() where T : SystemBase, new() => new T();

    public static void AddSystem<T>(this ICollection<SystemBase> systems)
            where T : SystemBase, new() => systems.Add(CreateSystem<T>());

    public static OnComponentAddedSignalsHandler<T> CreateComponentAddHandler<T>(ComponentHandler<T> handler)
            where T : unmanaged, IComponent => new(handler);

    public static void AddComponentAddHandler<T>(this ICollection<SystemBase> systems,
            ComponentHandler<T> handler)
            where T : unmanaged, IComponent {
      systems.Add(CreateComponentAddHandler(handler));
    }

    public static OnComponentRemovedSignalsHandler<T> CreateComponentRemoveHandler<T>(ComponentHandler<T> handler)
            where T : unmanaged, IComponent => new(handler);

    public static void AddComponentRemoveHandler<T>(this ICollection<SystemBase> systems,
            ComponentHandler<T> handler) where T : unmanaged, IComponent {
      systems.Add(CreateComponentRemoveHandler(handler));
    }
  }
}