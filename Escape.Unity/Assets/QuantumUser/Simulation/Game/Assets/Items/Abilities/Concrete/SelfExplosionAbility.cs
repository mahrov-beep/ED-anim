namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Prototypes;
  [Serializable]
  public unsafe class SelfExplosionAbilityItem : AbilityItemAsset {
    public AttackPrototype attackPrototype;

    public override Ability.AbilityState UpdateAbility(Frame f, EntityRef ownerRef, Ability* ability) {
      var state = base.UpdateAbility(f, ownerRef, ability);

      if (state.IsActiveStartTick) {
        var aoeAttack = f.Create();
        ObjectLifetime.Set(f, aoeAttack, FP._2);
        f.Add<Transform3D>(aoeAttack, out var attackTransform);

        TransformHelper.CopyPositionAndRotation(f, ownerRef, aoeAttack);
        
        Attack attack = default;
        attackPrototype.Materialize(f, ref attack);
        attack.SourceUnitRef = ownerRef;

        f.Set(aoeAttack, attack);
        
        var selfHeal = f.GetPointer<Health>(ownerRef);
        selfHeal->ApplyDamage(f, aoeAttack, selfHeal->InitialValue, EDamageType.None);
      }

      return state;
    }
  }
}