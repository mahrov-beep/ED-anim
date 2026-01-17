namespace Quantum {
  using System;
  using Photon.Deterministic;

  [Serializable]
  public unsafe class CombatLeaf : BTLeaf {
    public BTDataIndex stateIndex;
    public BTDataIndex endTimeIndex;

    const int STATE_AIMING   = 0;
    const int STATE_SHOOTING = 1;
    const int STATE_RESTING  = 2;

    public override void Init(BTParams p, ref AIContext c) {
      p.SetIntData(STATE_AIMING, stateIndex);
      p.SetFPData(FP._0, endTimeIndex);
    }

    public override void OnEnter(BTParams p, ref AIContext c) {
      base.OnEnter(p, ref c);

      var f    = p.FrameThreadSafe;
      var data = c.Data();

      var enemy = BotPerceptionHelper.GetClosestReactedEnemy(f, data.Bot, data.PerceptionMemory, data.Position);

      if (enemy == EntityRef.None) {
        return;
      }

      data.Bot->Intent.AttackTarget = enemy;
      data.Bot->SuppressFire = true;

      var combatConfig = f.FindAsset(data.Bot->CombatState);
      if (combatConfig == null) {
        return;
      }

      var gameTime = f.GetSingleton<BotSDKGlobals>().Data.GameTime;
      var endTime  = gameTime + combatConfig.AimDelaySeconds;

      p.SetIntData(STATE_AIMING, stateIndex);
      p.SetFPData(endTime, endTimeIndex);

      StartRepositioning(f, ref data, combatConfig);
    }

    protected override BTStatus OnUpdate(BTParams p, ref AIContext c) {
      var f    = p.FrameThreadSafe;
      var data = c.Data();

      if (data.Bot->Intent.AttackTarget == EntityRef.None) {
        return BTStatus.Failure;
      }

      var visibleEnemies = f.ResolveList(data.Bot->VisibleEnemies);
      if (!visibleEnemies.Contains(data.Bot->Intent.AttackTarget)) {
        return BTStatus.Failure;
      }

      var combatConfig     = f.FindAsset(data.Bot->CombatState);
      var difficultyConfig = f.FindAsset(data.Bot->StatsMultipliers);
      var gameTime         = f.GetSingleton<BotSDKGlobals>().Data.GameTime;
      var state            = p.GetIntData(stateIndex);
      var endTime          = p.GetFPData(endTimeIndex);

      if (state == STATE_AIMING) {
        data.Bot->SuppressFire = true;

        if (gameTime >= endTime) {
          BotNavMeshHelper.Stop(f, p.Entity, data.Pathfinder);
          data.InputContainer->ResetAllInput();

          p.SetIntData(STATE_SHOOTING, stateIndex);
          p.SetFPData(gameTime + difficultyConfig.BurstFireSeconds, endTimeIndex);
        }

        return BTStatus.Running;
      }

      if (state == STATE_SHOOTING) {
        data.Bot->SuppressFire = false;

        if (gameTime >= endTime) {
          p.SetIntData(STATE_RESTING, stateIndex);
          p.SetFPData(gameTime + difficultyConfig.BurstRestSeconds, endTimeIndex);

          StartRepositioning(f, ref data, combatConfig);
        }

        return BTStatus.Running;
      }

      if (state == STATE_RESTING) {
        data.Bot->SuppressFire = true;

        if (gameTime >= endTime) {
          var closestEnemy = BotPerceptionHelper.GetClosestReactedEnemy(f, data.Bot, data.PerceptionMemory, data.Position);
          if (closestEnemy != EntityRef.None) {
            data.Bot->Intent.AttackTarget = closestEnemy;
          }

          p.SetIntData(STATE_AIMING, stateIndex);
          p.SetFPData(gameTime + combatConfig.AimDelaySeconds, endTimeIndex);
        }

        return BTStatus.Running;
      }

      return BTStatus.Failure;
    }

    void StartRepositioning(FrameThreadSafe f, ref AIContextUser data, BotStateCombat combatConfig) {
      if (!f.TryGetPointer(data.Bot->Intent.AttackTarget, out Transform3D* enemyTransform)) {
        return;
      }

      var targetPos = BotNavMeshHelper.GetRandomPointAroundOnNavMesh(
        f,
        ref data.RNG,
        enemyTransform->Position,
        combatConfig.RepositionRadius);

      data.Bot->Intent.MovementTarget = targetPos;
      BotNavMeshHelper.SetTarget(f, data.Pathfinder, targetPos);
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
      data.Bot->SuppressFire = false;
      data.Bot->Intent.AttackTarget = EntityRef.None;
    }
  }
}
