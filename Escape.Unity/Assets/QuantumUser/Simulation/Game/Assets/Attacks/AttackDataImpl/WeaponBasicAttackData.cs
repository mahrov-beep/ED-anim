namespace Quantum {
  using Photon.Deterministic;
  using Physics3D;
  using UnityEngine;

  [System.Serializable]
  public unsafe partial class WeaponBasicAttackData : AttackData {
    public AssetRef<ReduceDamageByDistanceConfig> reduceDamageByDistance;

    [Header("Выше этого значения высоты от низа персонажа считается что голова")]
    [RangeEx(1f, 1.8f)]
    public FP heightThreshold = FP._1 + FP._0_50;

    public override void OnUpdate(Frame f, EntityRef attackRef, Attack* attack) {
      base.OnUpdate(f, attackRef, attack);

      var attackTransform = f.GetPointer<Transform3D>(attackRef);

      FPVector3 fromPosition = attackTransform->Position;
      FPVector3 direction    = attackTransform->Forward;
      FP        maxDistance  = attack->MaxDistance;

      var layerMask = PhysicsHelper.GetLagCompensatedUnitLayerMask(f, attack->SourceUnitRef);

      var hits = f.Physics3D.RaycastAll(fromPosition, direction, maxDistance, layerMask,
              QueryOptions.HitAll | QueryOptions.ComputeDetailedInfo);

      hits.SortCastDistance();

      bool needToDisable = false;
      for (var i = 0; i < hits.Count; i++) {
        var hit = hits[i];

        if (f.TryGetPointer(hit.Entity, out LagCompensationProxy* lagCompensationProxy)) {
          hit.SetHitEntity(lagCompensationProxy->Target);
        }

        var targetRef = CheckHit(f, hit, attackRef, out needToDisable);

        if (targetRef != EntityRef.None) {
          ReduceDamageByDistance(f, attackRef, attack);

          HandleHeadshot(f, attack, targetRef, hit);

          attack->HealthApplicator.ApplyOn(f, attackRef, targetRef);
          hitEffects.ApplyAll(f, targetRef);

          HandleHit(f, attackRef, targetRef, hit.Point, -direction);
        }

        if (needToDisable) {
          if (f.TryGetPointer(attackRef, out WeaponAttack* weaponAttack)) {
            var hitPoint  = hits[i].Point;
            var hitNormal = hits[i].IsDynamic ? default(NullableFPVector3) : hits[i].Normal;

            f.Events.OnWeaponAttackFired(weaponAttack->WeaponConfig, attack->SourceUnitRef, fromPosition, hitPoint, hitNormal);
          }

          break;
        }
      }

      if (!needToDisable) {
        if (f.TryGetPointer(attackRef, out WeaponAttack* weaponAttack)) {
          var hitPoint = fromPosition + direction * maxDistance;

          f.Events.OnWeaponAttackFired(weaponAttack->WeaponConfig, attack->SourceUnitRef, fromPosition, hitPoint, default);
        }
      }

      Deactivate(f, attackRef);
    }

    void HandleHeadshot(Frame f, Attack* attack, EntityRef targetRef, Hit3D hit) {
      var  tagetPosition = f.GetPointer<Transform3D>(targetRef)->Position;
      bool isHeadshot    = hit.Point.Y - tagetPosition.Y > heightThreshold;
      if (isHeadshot) {
        var sourceUnit = f.GetPointer<Unit>(attack->SourceUnitRef);
        var config     = sourceUnit->GetActiveWeaponConfig(f);
        if (config) {
          if (!attack->HealthApplicator.ValueIsPercent) {
            attack->HealthApplicator.Value *= config.headshotDamageMultiplier;
            attack->IsHeadshot             =  true;
          }
        }
      }
    }

    void ReduceDamageByDistance(Frame f, EntityRef attackRef, Attack* attack) {
      var config = f.FindAsset(reduceDamageByDistance);

      if (config == null) {
        f.LogWarning(attackRef, $"{nameof(ReduceDamageByDistanceConfig)} is null");
        return;
      }

      config.ReduceDamageByDistance(
              attack,
              TransformHelper.Distance(f, attack->SourceUnitRef, attackRef));
    }
  }
}