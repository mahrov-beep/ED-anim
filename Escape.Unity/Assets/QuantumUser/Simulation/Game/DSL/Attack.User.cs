namespace Quantum {
  using Photon.Deterministic;
  public unsafe partial struct Attack {
    public static ComponentHandler<Attack> OnCreate => static (f, e, c) => {
      if (c->AttackData == default) {
        f.LogError(e, $"Create attack without config!");
        return;
      }

      var config = f.FindAsset(c->AttackData);
      config.OnCreate(f, e, c);
    };

    public FP Damage => HealthApplicator.Value;
    public EDamageType DamageType => HealthApplicator.DamageType;
  }
}