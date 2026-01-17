using Photon.Deterministic;
using Sirenix.OdinInspector;

namespace Quantum {
  using UnityEngine;
  using static TransformHelper;

  [System.Serializable]
  public class PersistentAoEOptions {
    [Header("Performance")]
    [MinValue(1)] public int schedulePeriodTicks = 1;
    public bool checkLineOfSight = true;
    [ShowIf("checkLineOfSight"), MinValue(1)] public int losCheckEveryNTicks = 1;

    [Header("Hit output throttling")]
    public bool emitHitEvents;
    [ShowIf("emitHitEvents"), MinValue(1)] public int hitEventEveryNTicks = 3;
    public bool applyHitEffects = true;
    [ShowIf("applyHitEffects"), MinValue(1)] public int hitEffectsEveryNTicks = 1;
  }

  [System.Serializable]
  public unsafe class PersistentAreaOfEffectAttackData : AttackData {
    [ValidateInput(nameof(ValidateDamageCoefficient), "Must be between 0 and 1")]
    public FP minDamageDistanceCoefficient = FP._1;
    public FP radius = FP._4;

    [Header("Afterburn (applied when unit is in fire radius)")]
    public bool applyAfterburn;
    [ShowIf("applyAfterburn")] public FP afterburnDuration = FP._2;
    [ShowIf("applyAfterburn")] public FP afterburnDPS = FP._1;
    [ShowIf("applyAfterburn")] public EDamageType afterburnDamageType = EDamageType.Fire;
    [FoldoutGroup("PerformOptions")] public PersistentAoEOptions options = new PersistentAoEOptions();

    public override void OnUpdate(Frame f, EntityRef attackRef, Attack* attack) {
      base.OnUpdate(f, attackRef, attack);

      if (options.schedulePeriodTicks > 1) {
        const int moduloBase = 1024;
        if ((attackRef.Index % options.schedulePeriodTicks) != (f.Number % moduloBase % options.schedulePeriodTicks)) {
          return;
        }
      }

      var attackTransform = f.GetPointer<Transform3D>(attackRef);

      // DebugDrawHelper.DrawCircle(f, attackTransform->Position, radius, attackTransform->Rotation, ColorRGBA.Red, FP._0_50);

      var layerMask = PhysicsHelper.GetUnitLayerMask(f);
      var shape = new Shape3DConfig {
        ShapeType = Shape3DType.Sphere,
        SphereRadius = radius,
      };

      var hits = PhysicsHelper.OverlapShape(f, attackTransform, layerMask, shape);

      for (int i = 0; i < hits.Count; i++) {
        EntityRef targetRef = hits[i].Entity;
        if (!CanDamage(f, attackRef, targetRef)) {
          continue;
        }

        var hitTransform = f.GetPointer<Transform3D>(targetRef);
        bool behindObstacle = false;
        bool shouldCheckLoS = options.checkLineOfSight && (options.losCheckEveryNTicks <= 1 || (f.Number % options.losCheckEveryNTicks) == 0);
        if (shouldCheckLoS) {
          behindObstacle = !LineOfSightHelper.HasLineSight(f,
                  attackTransform->Position + FPVector3.Up,
                  hitTransform->Position + FPVector3.Up);
        }

        if (behindObstacle) {
          continue;
        }

        var isFirstEnter = false;
        if (!f.TryGet(targetRef, out InsidePersistentAoE insideAoE) || insideAoE.AttackRef != attackRef) {
          isFirstEnter = true;
          f.Set(targetRef, new InsidePersistentAoE {
            AttackRef = attackRef,
            LastUpdateTick = f.Number
          });
        } else {
          insideAoE.LastUpdateTick = f.Number;
          f.Set(targetRef, insideAoE);
        }

        var hitPoint = hitTransform->Position;
        var hitNormal = (hitTransform->Position - attackTransform->Position).Normalized * FP.Minus_1;

        var applicator = attack->HealthApplicator;

        if (applicator.Operation == HealthAttributeOperation.Damage && applicator.Appliance == HealthAttributeAppliance.OneTime) {
          applicator.Value *= f.DeltaTime;
        }

        if (minDamageDistanceCoefficient != FP._1) {
          var distanceSquared = DistanceSquared(attackTransform, hitTransform);
          var squaredCircleRadius = radius * radius;
          var damageDistMultiplier = FPMath.Lerp(
                  FP._1,
                  minDamageDistanceCoefficient,
                  distanceSquared / squaredCircleRadius);
          applicator.Value *= damageDistMultiplier;
        }

        bool shouldApplyHitEffects = options.applyHitEffects && (options.hitEffectsEveryNTicks <= 1 || (f.Number % options.hitEffectsEveryNTicks) == 0);
        if (shouldApplyHitEffects) {
          hitEffects.ApplyAll(f, targetRef);
        }
        applicator.ApplyOn(f, attackRef, targetRef);
        if (applyAfterburn && afterburnDPS > FP._0 && afterburnDuration > FP._0) {
          HealthBurnHelper.RefreshOrApplyContinuous(f, attack->SourceUnitRef, targetRef, afterburnDPS, afterburnDuration, afterburnDamageType);
        }
        bool shouldEmitHit = isFirstEnter || (options.emitHitEvents && (options.hitEventEveryNTicks <= 1 || (f.Number % options.hitEventEveryNTicks) == 0));
        if (shouldEmitHit) {
          HandleHit(f, attackRef, targetRef, hitPoint, hitNormal);
        }
      }
    }

    public override void OnCreate(Frame f, EntityRef attackRef, Attack* attack) {
      base.OnCreate(f, attackRef, attack);

      var attackTransform = f.GetPointer<Transform3D>(attackRef);
      f.Events.AttackPerformedSynced(attackRef, *attack, this, attackTransform->Position);
    }

    public override void Deactivate(Frame f, EntityRef attackRef) {
      f.Signals.OnDisableAttack(attackRef);
      f.Destroy(attackRef);
    }

    private bool ValidateDamageCoefficient(FP value) {
      return value >= FP._0 && value <= FP._1;
    }
  }
}


