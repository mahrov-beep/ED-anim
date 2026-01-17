namespace Quantum {
  using System.Linq;
  using UnityEngine;

  public unsafe class AIEnemiesNPCCreationSystem : SystemSignalsOnly, ISignalOnGameStart {
    public void OnGameStart(Frame f) {
      if (!f.IsVerified) {
        return;
      }

      if (!f.GameMode.fillRoomWithNpc) {
        return;
      }

      var filter = f.Filter<BotSpawnPoint>();

      while (filter.NextUnsafe(
               out var spawnRef,
               out var spawn)) {
        EntityRef botRef = f.Create(spawn->Prototype);

        f.Set(botRef, spawn->Team);
        f.Set(botRef, *f.GetPointer<Transform3D>(spawnRef));
        var config = f.FindAsset(f.GameMode.BotGlobalConfig);
        f.Set(botRef, new Bot {
          WayRef             = spawn->WayRef,
          BehaviourTreeAsset = spawn->TreeOverride != default ? spawn->TreeOverride : config.btAgentConfig.Tree,
          StatsMultipliers   = spawn->StatsMultipliersOverride != default ? spawn->StatsMultipliersOverride : config.StatsMultipliers,
          PatrolState        = spawn->PatrolStateOverride != default ? spawn->PatrolStateOverride : config.PatrolState,
          AlertState         = spawn->AlertStateOverride != default ? spawn->AlertStateOverride : config.AlertState,
          CombatState        = spawn->CombatStateOverride != default ? spawn->CombatStateOverride : config.CombatState,
          SuppressFire       = true,
        });

        SetupLoadout(f, botRef, spawn);

        var unit = f.GetPointer<Unit>(botRef);
        unit->ActiveWeaponRef = unit->ValidWeaponRef;

        var unitConfig = f.FindAsset(unit->Asset);
        if (unitConfig.IsSprintEnabled) {
          f.Set(botRef, new UnitFeatureSprintWithStamina {
            current = unitConfig.sprintSettings.maxStamina,
          });
        }

        f.Add(botRef, new CharacterFsm {
          SelfEntity = botRef,
        }, out var characterFsm);
        characterFsm->TryEnterState(f, new CharacterStateIdle());

        AIHelper.Botify(f, botRef);
        ApplyPerceptionModules(f, botRef, spawn, config);

        f.Signals.OnUnitSpawn(botRef);
      }
    }

    static void ApplyPerceptionModules(Frame f, EntityRef botRef, BotSpawnPoint* spawnPoint, BotGlobalConfig globalConfig) {
      var bot = f.GetPointer<Bot>(botRef);
      bot->VisionModule = spawnPoint->VisionModuleOverride != default
        ? spawnPoint->VisionModuleOverride
        : globalConfig.VisionModule;
      bot->HearingModule = spawnPoint->HearingModuleOverride != default
        ? spawnPoint->HearingModuleOverride
        : globalConfig.HearingModule;

      var memory = f.GetPointer<PerceptionMemory>(botRef);
      memory->MemoryModule = spawnPoint->MemoryModuleOverride != default
        ? spawnPoint->MemoryModuleOverride
        : globalConfig.MemoryModule;
    }

    static void SetupLoadout(Frame f, EntityRef characterRef, BotSpawnPoint* spawnPoint) {
      var botGlobalConfig = f.FindAsset(f.GameMode.BotGlobalConfig);

      GameSnapshotLoadout loadout;
      if (spawnPoint->LoadoutOverride != default) {
        var loadoutConfig = f.FindAsset(spawnPoint->LoadoutOverride);
        loadout = loadoutConfig.BuildLoadoutWithoutGuids(f);
        LoadoutGuidsHelper.AssignRandomDeterministicGuids(loadout, DeterministicGuid.Create(
          f.RuntimeConfig.DeterministicGuid, $"NpcLoadout:{characterRef}"));
      }
      else if (botGlobalConfig.Loadouts.Any()) {
        var loadoutConfig = botGlobalConfig.Loadouts.Random(f.RNG);
        loadout = loadoutConfig.BuildLoadoutWithoutGuids(f);
        LoadoutGuidsHelper.AssignRandomDeterministicGuids(loadout, DeterministicGuid.Create(
          f.RuntimeConfig.DeterministicGuid, $"NpcLoadout:{characterRef}"));
      }
      else {
        f.LogError(characterRef, "AI loadout configuration is empty");
        loadout = new GameSnapshotLoadout {
          SlotItems  = null,
          TrashItems = null,
        };
      }

      f.Add(characterRef, new CharacterLoadout(), out var characterLoadout);
      characterLoadout->SelfUnitEntity = characterRef;
      characterLoadout->CreateItemsFromRuntimeLoadout(f, loadout);
    }
  }
}