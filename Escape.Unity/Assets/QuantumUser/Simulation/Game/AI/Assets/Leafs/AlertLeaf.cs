namespace Quantum {
  using System;
  using Photon.Deterministic;

  [Serializable]
  public unsafe class AlertLeaf : BTLeaf {
    public BTDataIndex stateIndex;
    public BTDataIndex endTimeIndex;

    const int StateTurning = 0;
    const int StateMoving  = 1;

    public override void Init(BTParams p, ref AIContext c) {
      p.SetIntData(StateTurning, stateIndex);
      p.SetFPData(FP._0, endTimeIndex);
    }

    public override void OnEnter(BTParams p, ref AIContext c) {
      base.OnEnter(p, ref c);

      var f    = p.FrameThreadSafe;
      var data = c.Data();

      EntityRef alertTarget = EntityRef.None;

      var reactedEnemy = BotPerceptionHelper.GetFirstReactedEnemy(f, data.Bot, data.PerceptionMemory);
      if (reactedEnemy != EntityRef.None) {
        if (data.ActiveWeapon == null ||
            !BotPerceptionHelper.IsEnemyInAttackRange(f, data.Position, reactedEnemy, data.ActiveWeapon)) {
          alertTarget = reactedEnemy;
        }
      }

      if (alertTarget == EntityRef.None) {
        BotPerceptionHelper.TryGetFirstAlertEnemy(f, data.Bot, data.PerceptionMemory, out alertTarget);
      }

      if (alertTarget == EntityRef.None) {
        return;
      }

      data.Bot->Intent.AttackTarget = alertTarget;

      var config  = f.FindAsset(data.Bot->AlertState);
      var endTime = f.GetSingleton<BotSDKGlobals>().Data.GameTime + config.TurnWaitTime;

      p.SetIntData(StateTurning, stateIndex);
      p.SetFPData(endTime, endTimeIndex);
    }

    protected override BTStatus OnUpdate(BTParams p, ref AIContext c) {
      var f    = p.FrameThreadSafe;
      var data = c.Data();

      if (data.Bot->Intent.AttackTarget == EntityRef.None) {
        return BTStatus.Failure;
      }

      var config   = f.FindAsset(data.Bot->AlertState);
      var gameTime = f.GetSingleton<BotSDKGlobals>().Data.GameTime;
      var state    = p.GetIntData(stateIndex);
      var endTime  = p.GetFPData(endTimeIndex);

      if (state == StateTurning) {
        if (gameTime >= endTime) {
          var knownTargets = f.ResolveDictionary(data.PerceptionMemory->KnownTargets);
          if (!knownTargets.TryGetValue(data.Bot->Intent.AttackTarget, out var entry)) {
            return BTStatus.Failure;
          }

          var targetPos = BotNavMeshHelper.GetRandomPointAround(ref data.RNG, entry.LastKnownPosition, config.ApproachDistance);

          data.Bot->Intent.MovementTarget = targetPos;
          BotNavMeshHelper.SetTarget(f, data.Pathfinder, targetPos);

          var chaseEndTime = gameTime + config.ChaseTime;
          p.SetFPData(chaseEndTime, endTimeIndex);
          p.SetIntData(StateMoving, stateIndex);
        }

        return BTStatus.Running;
      }

      if (state == StateMoving) {
        if (WayHelper.IsNear(data.Position, data.Bot->Intent.MovementTarget, FP._2)) {
          return BTStatus.Success;
        }

        if (gameTime >= endTime) {
          return BTStatus.Failure;
        }

        if (!BotNavMeshHelper.IsMoving(data.Pathfinder)) {
          BotNavMeshHelper.SetTarget(f, data.Pathfinder, data.Bot->Intent.MovementTarget);
        }

        return BTStatus.Running;
      }

      return BTStatus.Failure;
    }

    public override void OnExit(BTParams p, ref AIContext c) {
      base.OnExit(p, ref c);
      Cleanup(p, ref c);
    }

    public override void OnAbort(BTParams p, ref AIContext c, BTAbort abortType) {
      base.OnAbort(p, ref c, abortType);
      Cleanup(p, ref c);
    }

    void Cleanup(BTParams p, ref AIContext c) {
      var f    = p.FrameThreadSafe;
      var data = c.Data();

      BotNavMeshHelper.Stop(f, p.Entity, data.Pathfinder);
      data.InputContainer->ResetAllInput();
      data.Bot->Intent.AttackTarget = EntityRef.None;
    }
  }
}
