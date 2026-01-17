namespace Quantum {
  using Photon.Deterministic;
  using Prototypes;

  public static unsafe class HealthApplicatorHelper {
    public static void ApplyOn(this HealthApplicatorPrototype applicatorPrototype, Frame f, EntityRef source, EntityRef target) {
      var applicator = new HealthApplicator();
      applicatorPrototype.Materialize(f, ref applicator);
      applicator.ApplyOn(f, source, target);
    }

    public static void ApplyOn(this HealthApplicator applicator, Frame f, EntityRef source, EntityRef target) {
      if (f.TryGetPointer<Health>(target, out var targetHealth)) {
        var amount = applicator.Value;

        if (applicator.ValueIsPercent) {
          amount = applicator.Value * FP._0_01 * targetHealth->MaxValue;
        }

        targetHealth->AddModifier(f, new HealthAttributeModifier {
                Appliance  = applicator.Appliance,
                Operation  = applicator.Operation,
                SourceRef  = source,
                Amount     = amount,
                Duration   = applicator.Duration,
                DamageType = applicator.DamageType,
        });
      }
    }

    public static void RefreshOrApplyOn(this HealthApplicator applicator, Frame f, EntityRef source, EntityRef target) {
      if (!f.TryGetPointer<Health>(target, out var targetHealth)) {
        return;
      }

      var amount = applicator.Value;
      if (applicator.ValueIsPercent) {
        amount = applicator.Value * FP._0_01 * targetHealth->MaxValue;
      }

      if (targetHealth->Modifiers.Ptr != default) {
        var list = f.ResolveList(targetHealth->Modifiers);
        for (int i = 0; i < list.Count; i++) {
          var existing = list.GetPointer(i);
          if (existing->Appliance == applicator.Appliance && 
              existing->Operation == applicator.Operation && 
              existing->DamageType == applicator.DamageType) {
            existing->Amount    = amount;
            existing->Duration  = applicator.Duration;
            existing->Timer     = applicator.Duration;
            existing->SourceRef = source;
            return;
          }
        }
      }
     
      targetHealth->AddModifier(f, new HealthAttributeModifier {
              Appliance  = applicator.Appliance,
              Operation  = applicator.Operation,
              SourceRef  = source,
              Amount     = amount,
              Duration   = applicator.Duration,
              DamageType = applicator.DamageType,
      });
    }

    public static void ApplyAll(this HealthApplicator[] applicators, Frame f, EntityRef source, EntityRef target) {
      foreach (var applicator in applicators) {
        applicator.ApplyOn(f, source, target);
      }
    }
  }
}