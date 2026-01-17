namespace Quantum {
  using System;
  using Prototypes;
  using UnityEngine;
  using Photon.Deterministic;

  [Serializable]
  public unsafe class ReconGrenadeAbilityItem : GrenadeAbilityItemBase {
    [Header("Recon Settings")]
    [Tooltip("Радиус обнаружения врагов")]
    public FP detectionRadius = FP._10 * 2;

    [Tooltip("Длительность эффекта разведки")]
    public FP effectDuration = FP._10;

    [Header("Explosion Settings")]
    public AttackPrototype attackPrototype;

    protected override void OnExplosion(Frame f, EntityRef ownerRef, EntityRef projectileRef) {
      var grenadeTransform = f.GetPointer<Transform3D>(projectileRef);

      var effectEntity = f.Create();
      f.Add<ReconEffect>(effectEntity, out var effect);

      if (f.TryGet(projectileRef, out Team ownerTeam)) {
        f.Add<Team>(effectEntity, out var effectTeam);
        *effectTeam = ownerTeam;
      }


      effect->Position = grenadeTransform->Position;
      effect->Radius = detectionRadius;
      effect->Duration = effectDuration;
      effect->StartTime = f.Number;
      effect->OwnerRef = ownerRef;

      if (attackPrototype != null) {
        SpawnAttackFromPrototype(f, ownerRef, grenadeTransform, attackPrototype);
      }
    }
  }
}

