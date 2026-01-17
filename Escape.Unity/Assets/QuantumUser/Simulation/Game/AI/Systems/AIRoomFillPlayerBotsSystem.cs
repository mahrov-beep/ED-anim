namespace Quantum {
  using System.Linq;
  using UnityEngine;
  public unsafe class AIRoomFillPlayerBotsSystem : SystemMainThread {

    public override void OnInit(Frame f) {
      var initializeData = f.Unsafe.GetOrAddSingletonPointer<InitializeData>();
      initializeData->FeelRoomWithBotDelay = f.GameMode.feelRoomWithBotDelay;
    }

    public override void Update(Frame f) {
      if (f.Global->GameState != EGameStates.Game) {
        return;
      }

      var initializeData = f.Unsafe.GetPointerSingleton<InitializeData>();
      if (initializeData->FeelRoomWithBotDelay.ProcessTimer(f)) {

        CreatePlayerBots(f);

        Disable(f);

      }
    }

    static void CreatePlayerBots(Frame f) {
      if (!f.IsVerified) {
        return;
      }

      if (!f.GameMode.fillRoomWithBots) {
        return;
      }

      Debug.Log($"PlayerConnectedCount = {f.PlayerConnectedCount}, PlayerCount = {f.MaxPlayerCount}");

      bool isFullRoom = f.PlayerConnectedCount >= f.MaxPlayerCount;
      if (isFullRoom) {
        return;
      }

      BotGlobalConfig botsGlobalConfig = f.FindAsset(f.GameMode.BotGlobalConfig);
      
      var connected      = (byte)f.PlayerConnectedCount;
      var maxPlayerCount = (byte)f.MaxPlayerCount;

      for (byte i = connected; i < maxPlayerCount; i++) {
        if (Application.isEditor) {
          Debug.Log($"Room filling: Player-bot was created instead of {(PlayerRef)i}");
        }

        var botRef = f.Global->CreatePlayerCharacter(f,
                loadoutSnapshot: BuildLoadout(f, botsGlobalConfig, i), out var spawnPoint
        );

        f.Set(botRef, new Bot {
                IsPlayerBot        = true,
                WayRef             = spawnPoint.wayRef,
                BehaviourTreeAsset = spawnPoint.TreeOverride != default ? spawnPoint.TreeOverride : botsGlobalConfig.btAgentConfig.Tree,
                StatsMultipliers   = spawnPoint.StatsMultipliersOverride != default ? spawnPoint.StatsMultipliersOverride : botsGlobalConfig.StatsMultipliers,
                PatrolState        = spawnPoint.PatrolStateOverride != default ? spawnPoint.PatrolStateOverride : botsGlobalConfig.PatrolState,
                AlertState         = spawnPoint.AlertStateOverride != default ? spawnPoint.AlertStateOverride : botsGlobalConfig.AlertState,
                CombatState        = spawnPoint.CombatStateOverride != default ? spawnPoint.CombatStateOverride : botsGlobalConfig.CombatState,
                SuppressFire       = true,
        });

        f.Set(botRef, new Team {
                Index = i,
        });

        f.Set(botRef, new NickName {
                Value = NickNameGenerator.Generate(f.RNG->Next(0, short.MaxValue), f.RNG->Next(0, short.MaxValue)),
        });

        AIHelper.Botify(f, botRef);
        ApplyPerceptionModules(f, botRef, spawnPoint, botsGlobalConfig);
      }
    }

    static GameSnapshotLoadout BuildLoadout(Frame f, BotGlobalConfig config, byte botIndex) {
      if (config.Loadouts.Any()) {
        var loadoutConfig = config.Loadouts.Random(f.RNG);
        var loadout = loadoutConfig.BuildLoadoutWithoutGuids(f);
        LoadoutGuidsHelper.AssignRandomDeterministicGuids(loadout, DeterministicGuid.Create(
                f.RuntimeConfig.DeterministicGuid, $"Player-bot Loadout {botIndex}"));
        return loadout;
      }

      return new GameSnapshotLoadout {
        TrashItems = null,
        SlotItems  = null,
      };
    }

    static void ApplyPerceptionModules(Frame f, EntityRef botRef, SpawnPoint spawnPoint, BotGlobalConfig globalConfig) {
      var bot = f.GetPointer<Bot>(botRef);
      bot->VisionModule = spawnPoint.VisionModuleOverride != default
        ? spawnPoint.VisionModuleOverride
        : globalConfig.VisionModule;
      bot->HearingModule = spawnPoint.HearingModuleOverride != default
        ? spawnPoint.HearingModuleOverride
        : globalConfig.HearingModule;

      var memory = f.GetPointer<PerceptionMemory>(botRef);
      memory->MemoryModule = spawnPoint.MemoryModuleOverride != default
        ? spawnPoint.MemoryModuleOverride
        : globalConfig.MemoryModule;
    }
  }
}