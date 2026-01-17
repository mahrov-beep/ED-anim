using UnityEngine.Scripting;

namespace Quantum {
  [Preserve]
  public unsafe class AttackSystem : SystemMainThreadFilter<AttackSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;
      public Attack*   Attack;
    }

    // Updates the Attack entities (projectiles, damage zones, etc)
    // Deactivates attacks if their TTL is finished, otherwise updates it's logic
    public override void Update(Frame f, ref Filter filter) {
      AttackData data = f.FindAsset<AttackData>(filter.Attack->AttackData.Id);
      
      if (TimerExpired(ref filter, data)) {
        data.Deactivate(f, filter.Entity);
        return;
      }

      filter.Attack->LifeTime += f.DeltaTime;

      data.OnUpdate(f, filter.Entity, filter.Attack);
    }

    bool TimerExpired(ref Filter filter, AttackData attackData) {
      return filter.Attack->LifeTime > attackData.TTL;
    }
  }
}
