namespace Quantum {
  using System;
  using BotSDK;
  using Photon.Deterministic;

  public static unsafe class AIHelper {
    public static bool TrySetBotify(Frame f, PlayerRef playerRef, bool botify) {
      var players = f.Filter<Unit>();
      while (players.NextUnsafe(out var e, out Unit* unit)) {
        if (unit->PlayerRef == playerRef) {
          if (botify) {
            Botify(f, e);
          }
          else {
            Debotify(f, e);
          }

          return true;
        }
      }

      return false;
    }

    public static void Botify(Frame f, EntityRef botRef) {
      BotGlobalConfig botGlobalConfig = f.FindAsset(f.GameMode.BotGlobalConfig);

      var btConfig = botGlobalConfig.btAgentConfig;

      var bot      = f.GetPointer<Bot>(botRef);
      var aiConfig = btConfig.AIConfig;

      f.Set(botRef, new BTAgent {
        Config = aiConfig,
        Tree   = bot->BehaviourTreeAsset,
      });

      if (UnityEngine.Application.isEditor) {
        var btAgent = f.GetPointer<BTAgent>(botRef);
        AddToDebugger(f, botRef, btAgent, bot->IsPlayerBot ? $"Player/{botRef.ToString()}" : $"NPC/{botRef.ToString()}");
      }

      if (!f.Has<NavMeshPathfinder>(botRef)) {
        var navMeshAgentConfig = f.FindAsset(f.SimulationConfig.Navigation.DefaultNavMeshAgent);
        var pathfinder         = NavMeshPathfinder.Create(f, botRef, navMeshAgentConfig);
        f.Set(botRef, pathfinder);
      }

      f.Add(botRef, out NavMeshSteeringAgent* steering);
      f.Add(botRef, out NavMeshAvoidanceAgent* avoidance);

      if (!f.Has<PerceptionMemory>(botRef)) {
        f.Set(botRef, new PerceptionMemory());
      }

      f.LogDebug(botRef, nameof(Botify));
    }

    public static void Debotify(Frame f, EntityRef e) {
      f.Remove<Bot>(e);

      f.Remove<NavMeshPathfinder>(e);
      f.Remove<NavMeshSteeringAgent>(e);
      f.Remove<NavMeshAvoidanceAgent>(e);

      f.Remove<PerceptionMemory>(e);

      f.LogDebug(e, nameof(Debotify));
    }

    static void AddToDebugger<T>(Frame f, EntityRef e, T* c, string label = null)
      where T : unmanaged, IComponent, IBotSDKDebugInfoProvider {
      var componentIndex = ComponentTypeId.GetComponentIndex(typeof(T));

      Func<DelegateGetDebugInfo> debugInfoGetter = (*c).GetDebugInfo;

      BotSDKDebuggerSystemCallbacks.SetEntityDebugLabel?
        .Invoke(f, e, label, componentIndex, debugInfoGetter);
    }
  }
}