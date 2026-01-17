namespace Quantum {
  using Photon.Deterministic;

  public static unsafe class HealthBurnHelper {    
    public static void RefreshOrApplyFireBurn(Frame f, EntityRef source, EntityRef target, FP dps, FP duration) {
      RefreshOrApplyContinuous(f, source, target, dps, duration, EDamageType.Fire);
    }

    public static void RefreshOrApplyContinuous(Frame f, EntityRef source, EntityRef target, FP dps, FP duration, EDamageType damageType) {
      if (dps <= FP._0 || duration <= FP._0) {
        return;
      }

      var applicator = new HealthApplicator {
              Value      = dps * duration,
              DamageType = damageType,
              Appliance  = HealthAttributeAppliance.Continuous,
              Operation  = HealthAttributeOperation.Damage,
              Duration   = duration,
      };

      applicator.RefreshOrApplyOn(f, source, target);
    }
  }
}