namespace Quantum {
  using System;

  [Serializable]
  public unsafe class HasEnemyInAlertZone : BTDecorator {
    public override bool CheckConditions(BTParams p, ref AIContext c) {
      var f    = p.FrameThreadSafe;
      var data = c.Data();

      var knownTargets = f.ResolveDictionary(data.PerceptionMemory->KnownTargets);
      if (knownTargets.Count == 0) {
        return false;
      }

      var reactedEnemy = BotPerceptionHelper.GetFirstReactedEnemy(f, data.Bot, data.PerceptionMemory);
      if (reactedEnemy != EntityRef.None && data.ActiveWeapon != null) {
        if (BotPerceptionHelper.IsEnemyInAttackRange(f, data.Position, reactedEnemy, data.ActiveWeapon)) {
          return false;
        }
      }

      return true;
    }
  }
}