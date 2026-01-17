namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Prototypes;
  using UnityEngine;

  [Serializable]
  public unsafe class FlashbangAbilityItem : GrenadeAbilityItemBase {
    [Header("Flashbang Effect Settings")]
    public FP blindRadius = FP._10;
    public FP blindDuration = FP._5;

    [Header("Explosion Settings")]
    public AttackPrototype attackPrototype;

    protected override void OnExplosion(Frame f, EntityRef ownerRef, EntityRef projectileRef) {
      var flashbangTransform = f.GetPointer<Transform3D>(projectileRef);
      SpawnAttackFromPrototype(f, ownerRef, flashbangTransform, attackPrototype);

      ApplyFlashbangEffect(f, flashbangTransform->Position);
    }

    void ApplyFlashbangEffect(Frame f, FPVector3 explosionPosition) {
      var filter = f.Filter<Unit, Transform3D>();

      while (filter.NextUnsafe(out var entityRef, out var unit, out var transform)) {
        var distance = FPVector3.Distance(transform->Position, explosionPosition);
        if (distance > blindRadius) {
          continue;
        }

        bool hasLoS = LineOfSightHelper.HasLineSightFast(f, explosionPosition, entityRef);

        var distanceStrength = FP._1 - (distance / blindRadius);

        var toExplosion = (explosionPosition - transform->Position).Normalized;
        var forward = transform->Rotation * FPVector3.Forward;
        var dot = FPVector3.Dot(forward, toExplosion);
        var facingStrength = FPMath.Clamp01((dot + FP._1) * FP._0_50);

        var blindStrength = distanceStrength * (hasLoS ? FP._1 : FP._0_25) * facingStrength;
        blindStrength = FPMath.Clamp01(blindStrength);

        if (blindStrength > FP._0) {
          ApplyBlindEffect(f, entityRef, blindStrength);
        }
      }
    }

    void ApplyBlindEffect(Frame f, EntityRef targetRef, FP blindStrength) {
      if (!f.Has<FlashbangBlindEffect>(targetRef)) {
        f.Add<FlashbangBlindEffect>(targetRef);
      }

      var blindEffect = f.GetPointer<FlashbangBlindEffect>(targetRef);
      blindEffect->Duration = blindDuration * blindStrength;
      blindEffect->Strength = blindStrength;
      blindEffect->StartTime = f.Number;
    }
  }
}