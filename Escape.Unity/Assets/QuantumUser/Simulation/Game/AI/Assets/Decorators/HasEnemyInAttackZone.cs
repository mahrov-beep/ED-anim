namespace Quantum {
  using System;

  [Serializable]
  public unsafe class HasEnemyInAttackZone : BTDecorator {

    public override bool CheckConditions(BTParams p, ref AIContext c) {
      var f = p.FrameThreadSafe;
      var data = c.Data();

      if (data.ActiveWeapon == null) {
        return false;
      }

      var enemy = BotPerceptionHelper.GetClosestReactedEnemy(f, data.Bot, data.PerceptionMemory, data.Position);
      if (enemy == EntityRef.None) {
        return false;
      }

      return BotPerceptionHelper.IsEnemyInAttackRange(f, data.Position, enemy, data.ActiveWeapon);
    }
  }
}
