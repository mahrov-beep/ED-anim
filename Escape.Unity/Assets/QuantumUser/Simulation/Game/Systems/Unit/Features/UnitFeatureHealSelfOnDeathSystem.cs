namespace Quantum {
  using Photon.Deterministic;

  public unsafe class UnitFeatureHealSelfOnDeathSystem : SystemMainThreadFilter<UnitFeatureHealSelfOnDeathSystem.Filter>,
    ISignalOnUnitDead, ISignalOnUnitHeal {
    public struct Filter {
      public EntityRef Entity;

      public Unit*                       Unit;
      public Health*                     Health;
      public UnitFeatureHealSelfOnDeath* Feature;
    }

    public override void Update(Frame f, ref Filter filter) {
      filter.Feature->RespawnTimer.Tick(f.DeltaTime);

      if (filter.Feature->RespawnTimer.IsDone) {
        filter.Feature->RespawnTimer.Reset();
        
        filter.Feature->Applicator.ApplyOn(f, filter.Entity, filter.Entity);
      }
    }

    public void OnUnitDead(Frame f, EntityRef e) {
      if (f.TryGetPointer<UnitFeatureHealSelfOnDeath>(e, out var feature)) {
        feature->RespawnTimer.Start(feature->HealDelaySeconds);
      }
    }

    public void OnUnitHeal(Frame f, EntityRef source, EntityRef target, FP value) {
      if (f.TryGetPointer<UnitFeatureHealSelfOnDeath>(target, out var feature)) {
        feature->RespawnTimer.Reset();
      }
    }
  }
}