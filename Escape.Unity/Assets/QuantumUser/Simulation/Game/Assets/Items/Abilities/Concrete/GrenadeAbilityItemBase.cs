namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Prototypes;
  using UnityEngine;

  public enum GrenadeDetonationType {
    /// <summary>Explodes on collision (molotov)</summary>
    OnCollision = 0,
    /// <summary>Explodes after delay (gas, smoke)</summary>
    OnTimer = 1
  }

  [Serializable]
  public abstract unsafe class GrenadeAbilityItemBase : AbilityItemAsset {
    [Header("Grenade Settings")]
    public AssetRef<EntityPrototype> grenadePrototype;
    public FPVector3 throwOffset;
    public FP throwAngleDegrees = FP._1 * 35;
    public FP maxThrowDistance = FP._10 * 2;
    public FP throwSpeed = FP._10 * 2;
    public FP inheritVelocityFactor = FP._1;
    
    [Header("Detonation Settings")]
    [Tooltip("Grenade detonation type")]
    public GrenadeDetonationType detonationType = GrenadeDetonationType.OnTimer;
    
    [Tooltip("Time before grenade explodes (OnTimer type only)")]
    [DrawIf("detonationType", (long)GrenadeDetonationType.OnCollision, CompareOperator.NotEqual, mode: DrawIfMode.Hide)]
    public FP explosionDelay = FP._3;
    
    public override void Reset() {
      base.Reset();
      durationSec = detonationType == GrenadeDetonationType.OnTimer ? explosionDelay : FP._10;
    }

    public override Ability.AbilityState UpdateAbility(Frame f, EntityRef ownerRef, Ability* ability) {
      durationSec = detonationType == GrenadeDetonationType.OnTimer ? explosionDelay : FP._10;
      
      var state = base.UpdateAbility(f, ownerRef, ability);
      var animation = f.GetPointer<AnimationTriggers>(ownerRef);

      var shouldPlayThrowAnim = state.IsDelayed || state.IsActiveStartTick;
      if (shouldPlayThrowAnim) {
        animation->Throw = true;
      }

      if (state.IsActiveStartTick) {
        ThrowProjectile(f, ownerRef);
      }

      if (state.IsActiveEndTick) {
        animation->Throw = false;
      }

      var unit = f.GetPointer<Unit>(ownerRef);
      var projectileRef = unit->ActiveAbilityInfo.AbilityEffectRef;

      if (f.Exists(projectileRef)) {
        bool shouldExplode = false;
        
        if (detonationType == GrenadeDetonationType.OnTimer) {
          shouldExplode = state.IsActiveEndTick;
        } else if (detonationType == GrenadeDetonationType.OnCollision) {
          shouldExplode = TryTriggerExplosionOnCollision(f, ownerRef, projectileRef) || state.IsActiveEndTick;
        }
        
        if (shouldExplode) {
          TriggerExplosion(f, ownerRef, projectileRef);
          f.Destroy(projectileRef);
        }
      }

      return state;
    }

    protected virtual bool TryTriggerExplosionOnCollision(Frame f, EntityRef ownerRef, EntityRef projectileRef) {
      if (!f.TryGet<PhysicsCollider3D>(projectileRef, out var collider)) {
        return false;
      }

      var projectileTransform = f.GetPointer<Transform3D>(projectileRef);
      var hits = f.Physics3D.OverlapShape(projectileTransform->Position, projectileTransform->Rotation, collider.Shape, -1);
      
      for (int i = 0; i < hits.Count; i++) {
        var hit = hits[i];
        if (hit.Entity != ownerRef && hit.Entity != projectileRef && (hit.Entity.IsValid || hit.StaticColliderIndex >= 0)) {
          return true;
        }
      }
      
      return false;
    }

    protected void TriggerExplosion(Frame f, EntityRef ownerRef, EntityRef projectileRef) {
      OnExplosion(f, ownerRef, projectileRef);
    }

    protected abstract void OnExplosion(Frame f, EntityRef ownerRef, EntityRef projectileRef);

    protected EntityRef SpawnAttackFromPrototype(Frame f, EntityRef ownerRef, Transform3D* sourceTransform, AttackPrototype prototype) {
      if (prototype == null) {
        return default;
      }

      Attack attack = default;
      prototype.Materialize(f, ref attack);
      attack.SourceUnitRef = ownerRef;

      var attackRef = f.Create();
      f.Add<Transform3D>(attackRef, out var attackTransform);
      TransformHelper.CopyPositionAndRotation(from: sourceTransform, to: attackTransform);

      f.Set(attackRef, attack);
      return attackRef;
    }

    protected virtual void ThrowProjectile(Frame f, EntityRef ownerRef) {
      f.TryGetPointers(ownerRef, out KCC* ownerKcc, out Unit* unit);

      var aim = f.GetPointer<UnitAim>(ownerRef);
      var origin = f.GameModeAiming.GetAimOrigin(f, ownerRef);
      var direction = (aim->AimCurrentPosition - origin).Normalized;
      var rotation = FPQuaternion.LookRotation(direction, FPVector3.Up);

      var projectileRef = f.Create(grenadePrototype);
      unit->ActiveAbilityInfo.AbilityEffectRef = projectileRef;

      var projectileTransform = f.GetPointer<Transform3D>(projectileRef);
      projectileTransform->Position = origin + rotation * throwOffset;
      projectileTransform->Rotation = rotation;

      if (f.TryGet(ownerRef, out Team ownerTeam)) {
        f.Set(projectileRef, ownerTeam);
      }

      var projectileBody = f.GetPointer<PhysicsBody3D>(projectileRef);
      var launchVelocity = direction * throwSpeed + ownerKcc->Data.RealVelocity * inheritVelocityFactor;
      projectileBody->AddLinearImpulse(launchVelocity * projectileBody->Mass);
    }

    protected virtual FP GetThrowDistance(Frame f, EntityRef ownerRef) {
      f.TryGetPointers(ownerRef, out KCC* kcc, out UnitAim* aim);
      var distance = FPVector3.Distance(kcc->Position, aim->AimCurrentPosition);
      return FPMath.Min(distance, maxThrowDistance);
    }

    protected static FP CalculateLaunchSpeed(FP distance, FP angleDegrees, FP gravity) {
      gravity = FPMath.Abs(gravity);
      var angleRadians = angleDegrees * FP.Deg2Rad;
      var sin2Angle = FPMath.Sin(FP._2 * angleRadians);

      if (sin2Angle == FP._0) {
        throw new Exception("Невозможный угол броска: " + angleDegrees);
      }

      return FPMath.Sqrt((distance * gravity) / sin2Angle);
    }
  }
}
