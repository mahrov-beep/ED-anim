using Photon.Deterministic;
using Sirenix.OdinInspector;

namespace Quantum {
  using UnityEngine;
  using static TransformHelper;
  [System.Serializable]
  public abstract unsafe partial class AreaOfEffectAttackData : AttackData {

    [Header("When 1 damage will be equals in all area")]
    [ValidateInput(nameof(ValidateDamageCoefficient), "Must be between 0 and 1")]
    public FP minDamageDistanceCoefficient = FP._1; //
    public FP radius = FP._4;

    public override void OnUpdate(Frame f, EntityRef attackRef, Attack* attack) {
      base.OnUpdate(f, attackRef, attack);
      PerformDamage(f, attackRef, attack);
    }

    protected void PerformDamage(Frame f, EntityRef attackRef, Attack* attack) {
      var sourceRef = attack->SourceUnitRef;
      var attackTransform = f.GetPointer<Transform3D>(attackRef);

      // DebugDrawHelper.DrawCircle(f, attackTransform->Position, radius, attackTransform->Rotation, ColorRGBA.Red, FP._5);

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
        bool behindObstacle = !LineOfSightHelper.HasLineSight(f,
                attackTransform->Position/*.XOZ*/ + FPVector3.Up,
                hitTransform->Position/*.XOZ*/ + FPVector3.Up);

        if (behindObstacle) {
          continue;
        }

        var hitPoint = hitTransform->Position;
        var hitNormal = (hitTransform->Position - attackTransform->Position).Normalized * FP.Minus_1;

        hitEffects.ApplyAll(f, targetRef);
        OnApplyDamage(f, sourceRef, targetRef, attackRef);
        HandleHit(f, attackRef, targetRef, hitPoint, hitNormal);
      }

      Deactivate(f, attackRef);
    }

    public void OnApplyDamage(Frame f, EntityRef sourceRef, EntityRef targetRef, EntityRef attackRef) {

      var attack = f.GetPointer<Attack>(attackRef);

      if (minDamageDistanceCoefficient == FP._1) {
        attack->HealthApplicator.ApplyOn(f, attackRef, targetRef);
        return;
      }

      var attackTransform = f.GetPointer<Transform3D>(attackRef);
      var targetTransform = f.GetPointer<Transform3D>(targetRef);

      var distanceSquared = DistanceSquared(attackTransform, targetTransform);
      var squaredCircleRadius = radius * radius;

      //damageDistMultiplier меняется от 1 до 0 в инспекторе
      var damageDistMultiplier = FPMath.Lerp(
              FP._1,
              minDamageDistanceCoefficient,
              distanceSquared / squaredCircleRadius);

      var applicator = attack->HealthApplicator;

      applicator.Value *= damageDistMultiplier;
      applicator.ApplyOn(f, sourceRef, targetRef);
    }
    private bool ValidateDamageCoefficient(FP value) {
      return value >= FP._0 && value <= FP._1;
    }
  }
}