namespace Quantum {
  using Photon.Deterministic;
  using Sirenix.OdinInspector;

  [System.Serializable]
  public struct Effect {
    public string             Name;
    public EAttributeType     AttributeType;
    
    [TableColumnWidth(60, false)]  public FP                 Value;
    [TableColumnWidth(80, false)]  public EModifierOperation Operation;
    [TableColumnWidth(140, false)] public EModifierAppliance Appliance;

    [DrawIf(nameof(Appliance), (long)EModifierAppliance.OneTime, CompareOperator.Greater)]
    [TableColumnWidth(60, false)] 
    public FP Duration;
  }

  public static unsafe class EffectsHelper {
    public static void ApplyAll(this Effect[] effects, Frame f, EntityRef e) {
      foreach (var effect in effects) {
        effect.Apply(f, e);
      }
    }

    public static void Apply(this Effect effect, Frame f, EntityRef e) {
      if (effect.Operation == EModifierOperation.None) {
        f.LogWarning(e, $"not valid operation, can't apply '{effect.Name}'");
        return;
      }

      if (!f.Has<Attributes>(e)) {
        f.LogWarning(e, $"has not attributes, can't apply '{effect.Name}'");
        return;
      }

      var value = effect.Value;

      if (value < 0) {
        f.LogError(e, $"Tried to apply effect '{effect.Name}' with negative value. Ignored");
        return;
      }

      effect.AttributeType.ChangeAttribute(f, e,
              effect.Appliance,
              effect.Operation,
              value,
              effect.Duration);
    }
  }
}