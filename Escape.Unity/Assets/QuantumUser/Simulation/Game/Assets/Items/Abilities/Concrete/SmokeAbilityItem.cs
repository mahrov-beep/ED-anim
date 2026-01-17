namespace Quantum {
  using System;
  using Prototypes;
  using UnityEngine;

  [Serializable]
  public unsafe class SmokeAbilityItem : GrenadeAbilityItemBase {
    [Header("Explosion Settings")]
    public AttackPrototype attackPrototype;

    protected override void OnExplosion(Frame f, EntityRef ownerRef, EntityRef projectileRef) {
      var smokeTransform = f.GetPointer<Transform3D>(projectileRef);
      SpawnAttackFromPrototype(f, ownerRef, smokeTransform, attackPrototype);
    }
  }
}