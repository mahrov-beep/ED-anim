namespace Quantum {
  using Core;
  using Photon.Deterministic;
  using UnityEngine;
public unsafe partial struct Weapon {
  public WeaponItemAsset GetConfig(FrameBase f) {
    WeaponItemAsset config = f.FindAsset(Config);
    if (config == null) {
      Debug.LogError("Couldn't find config");
    }
    return config;
  }  

  public WeaponItemAsset GetConfig(FrameThreadSafe f) {
    WeaponItemAsset config = f.FindAsset(Config);
    if (config == null) {
      Debug.LogError("Couldn't find config");
    }
    return config;
  }

  public bool CanShoot(FrameBase f, EntityRef ownerRef, EntityRef targetRef) {
    var config = GetConfig(f);
    var isTurret = f.Has<Turret>(ownerRef);

    if (FireRateTimer.IsSet && FireRateTimer.IsRunning(f)) {
      return false;
    }

    if (IsReloading) {
      return false;
    }

    if (IsEmptyMagazine && config.magazineAmmoCount > 0) {
      return false;
    }

    if (f.TryGetPointer(ownerRef, out Unit* ownerUnit)) {
      var preShotAimingSeconds = CurrentStats.preShotAimingSeconds;
      
      if (f.TryGetPointers(ownerRef, out Bot* bot, out AIBlackboardComponent* bb, out BTAgent* agent)) {
        // TODO вспомнить зачем это, выглядит плохо
        var botConfig     = f.FindAsset(agent->Config);
        var botPreShotSec = botConfig.Get(AIConstants.ConfigSettings.SHOOT_PRE_SHOT_AIMING_SECONDS);
        if (botPreShotSec != null) {
          preShotAimingSeconds += botPreShotSec.Value.FP;
        }
      }
      
      if (ownerUnit->WeaponAimSecondsElapsed < preShotAimingSeconds) {
        return false;
      }
    }

    if (!f.TryGetPointer(ownerRef, out Transform3D* ownerTransform)) {
      return false;
    }

    if (!f.TryGetPointer(targetRef, out Transform3D* targetTransform)) {
      return false;
    }

    var attackDistance = CurrentStats.attackDistance.AsFP;
    var distanceSqr = (ownerTransform->Position - targetTransform->Position).SqrMagnitude;
    if (distanceSqr > attackDistance * attackDistance) {
      return false;
    }
    
    var ownerHeight = UnitColliderHeightHelper.GetCurrentHeight(f, ownerRef);
    FPVector3 shootStartPosition = ownerHeight > FP._0
            ? ownerTransform->Position + FPVector3.Up * (ownerHeight * FP._0_50)
            : ownerTransform->Position + ownerTransform->Rotation * config.shotOriginOffset;

    var targetHeight = UnitColliderHeightHelper.GetCurrentHeight(f, targetRef);
    FP targetAimOffset = config.shotTargetOffsetY > FP._0
            ? config.shotTargetOffsetY
            : (targetHeight > FP._0 ? targetHeight * FP._0_50 : FP._0);
    FPVector3 targetPosition = targetTransform->Position + FPVector3.Up * targetAimOffset;

    var layerMask = PhysicsHelper.GetBlockRaycastLayerMask(f);
    Physics3D.HitCollection3D hits = f.Physics3D.LinecastAll(
            shootStartPosition, 
            targetPosition, 
            layerMask);

    hits.SortCastDistance();

    for (var i = 0; i < hits.Count; i++) {
      var hit = hits[i];

      if (hit.IsStatic) { 
        // DebugDrawHelper.DrawLine(f, shootStartPosition, targetPosition, ColorRGBA.Yellow, FP._0_75);
        return false;
      }

      if (hit.IsDynamic) {
        var hitEntity = hit.Entity;
        if (hitEntity != EntityRef.None) {
          if (hitEntity == ownerRef) {
            continue;
          }

          if (f.TryGetPointer<Team>(hit.Entity, out var hitTeam)) {
            var ownerRefTeam = f.GetPointer<Team>(ownerRef);
            if (ownerRefTeam->Equals(hitTeam)) {
              return false;
            }
          }
        }
      }
    }

    // DebugDrawHelper.DrawLine(f, shootStartPosition, targetPosition, ColorRGBA.Red, FP._1);

    return true;
  }
}
}
