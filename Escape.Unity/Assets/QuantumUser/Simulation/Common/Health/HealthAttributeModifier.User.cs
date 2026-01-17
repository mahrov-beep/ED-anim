namespace Quantum {
  using Photon.Deterministic;
  using static HealthAttributeOperation;

  public partial struct HealthAttributeModifier {
    public void Init(Frame f) {
      Timer = Duration;
    }

    public void Tick(Frame f, out bool ttlOver) {
      if (Timer > 0 || Appliance == HealthAttributeAppliance.OneTime) {
        Timer -= f.DeltaTime;
        if (Timer <= 0) {
          ttlOver = true;
          return;
        }
      }

      ttlOver = false;
    }

    public void DeApply(Frame f, ref Health health) {
      var reversedOperation = Operation switch {
        Heal => Damage,
        Damage => Heal,
        IncreaseHealth => DecreaseHealth,
        DecreaseHealth => IncreaseHealth,
        IncreaseMaxHealth => DecreaseMaxHealth,
        DecreaseMaxHealth => IncreaseMaxHealth,
        _ => None,
      };

      Apply(f, ref health, reversedOperation);
    }

    public void Apply(Frame f, ref Health health, HealthAttributeOperation forcedOperation = None) {
      var valueToApply = Amount;

      if (Appliance == HealthAttributeAppliance.Continuous) {
        // корректируем DeltaTime чтобы на последнем кадре работы атрибута добавить более точное значение
        var dt = FPMath.Clamp(Timer, FP._0, f.DeltaTime);
        valueToApply = Amount * dt / FPMath.Max(Duration, FP.Epsilon);
      }

      var operation = forcedOperation != None ? forcedOperation : Operation;

      switch (operation) {
        case Heal:
          health.ApplyHeal(f, SourceRef, valueToHeal: valueToApply);
          break;

        case Damage:
          health.ApplyDamage(f, SourceRef, baseDamage: valueToApply, DamageType);
          break;

        case IncreaseHealth:
          health.ModifyHealth(f, valueToAdd: +valueToApply);
          break;

        case DecreaseHealth:
          health.ModifyHealth(f, valueToAdd: -valueToApply);
          break;

        case IncreaseMaxHealth:
          health.ModifyMaxHealth(f, valueToAdd: +valueToApply);
          break;

        case DecreaseMaxHealth:
          health.ModifyMaxHealth(f, valueToAdd: -valueToApply);
          break;
      }
    }
  }
}