namespace Quantum {
  using System;
  using Prototypes;
  using Sirenix.OdinInspector;
  [Serializable]
  public class HealAbilityItem : AbilityItemAsset {
    [InlineProperty(LabelWidth = 100)]
    public HealthApplicatorPrototype heal;

    public override unsafe Ability.AbilityState UpdateAbility(Frame f, EntityRef ownerRef, Ability* ability) {
      var state = base.UpdateAbility(f, ownerRef, ability);

      if (state.IsActiveStartTick) {
        heal.ApplyOn(f, ownerRef, ownerRef);
      }

      return state;
    }
  }
}