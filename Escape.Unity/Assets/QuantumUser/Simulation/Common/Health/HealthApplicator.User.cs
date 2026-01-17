namespace Quantum {
  using Photon.Deterministic;
  public partial struct HealthApplicator {
    public static HealthApplicator CreateDamage(FP damage, EDamageType damageType) {
      HealthApplicator applicator = default;

      applicator = new HealthApplicator {
              Value      = damage,
              DamageType = damageType,
              Appliance  = HealthAttributeAppliance.OneTime,
              Operation  = HealthAttributeOperation.Damage,
      };

      return applicator;
    }

    public static HealthApplicator CreateBurn(FP damage, FP duration) {
      HealthApplicator applicator = default;

      applicator = new HealthApplicator {
              Value      = damage,
              DamageType = EDamageType.Fire,
              Appliance  = HealthAttributeAppliance.Continuous,
              Operation  = HealthAttributeOperation.Damage,
              Duration   = duration,
      };

      return applicator;
    }
  }
}