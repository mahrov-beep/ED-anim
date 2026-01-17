namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;
  using UnityEngine.Pool;

  [Serializable]
  public abstract unsafe class AimingAsset : AssetObject {
    [RequiredRef] public AssetRef<EntityPrototype> unitSpectatorCamera;
    [RequiredRef] public AssetRef<EntityPrototype> unitAim;

    public FPVector3 characterTorsoOffset    = new FPVector3(0, FP._1 + FP._0_25, 0);
    public FPVector3 characterShoulderOffset = new FPVector3(FP._0_25, FP._1 + FP._0_50 + FP._0_25, FP._0);

    readonly ObjectPool<List<PhysicsHelper.PhysicsTarget>> targetListPool = new(
      () => new List<PhysicsHelper.PhysicsTarget>(16), actionOnRelease: it => it.Clear());

    public abstract bool UseTopDownQuantumPredictionCulling { get; }

    public abstract void UpdateSpectatorCamera(Frame f, EntityRef unitEntity, InputContainer* input,
      CharacterSpectatorCamera* spectatorCamera);

    public abstract void UpdateAim(Frame f, EntityRef unitEntity, UnitAim* unitAim, CharacterSpectatorCamera* spectatorCamera);

    public abstract void ApplySpectatorEntity(Frame f, EntityRef unitEntity, CharacterSpectatorCamera* spectatorCamera);

    public FPVector3 GetAimOrigin(Frame f, EntityRef unitEntity) {
      UnitHelper.CalculateRig(f, unitEntity, this.characterTorsoOffset, this.characterShoulderOffset, FPVector3.Zero,
        out _, out var worldShoulder, out _);
      return worldShoulder;
    }

    public virtual FP GetCharacterRotation(Frame f, CharacterSpectatorCamera* spectatorCamera) {
      return 
         FPMathHelper.SmoothDampUnclamped(
         current: spectatorCamera->CharacterCurrentRotation,
         target: spectatorCamera->SpectatorCameraDesiredRotation.Yaw,
         currentVelocity: ref spectatorCamera->CharacterCurrentVelocity,
         smoothTime: (FP)100 / 1000, // скорость поворота персонажа
         deltaTime: f.DeltaTime
       );
    }
    
    public virtual FP GetCharacterPitchRotation(Frame f, CharacterSpectatorCamera* spectatorCamera) {
      return 
        FPMathHelper.SmoothDampUnclamped(
          current: spectatorCamera->CharacterCurrentPitchRotation,
          target: spectatorCamera->SpectatorCameraDesiredRotation.Pitch,
          currentVelocity: ref spectatorCamera->CharacterCurrentVelocity,
          smoothTime: (FP)90 / 1000, // скорость поворота персонажа
          deltaTime: f.DeltaTime
        );
    }



    protected unsafe delegate bool AimBlockEvaluator(Frame f, EntityRef unitEntity, CharacterSpectatorCamera* spectatorCamera, FPVector3 aimOrigin, FPVector3 aimTarget);

    protected void UpdateStandardAim(
      Frame f,
      EntityRef unitEntity,
      UnitAim* unitAim,
      CharacterSpectatorCamera* spectatorCamera,
      FPVector3 aimOrigin,
      FPVector3 aimTarget,
      bool autoAimEnabled,
      AimBlockEvaluator? aimBlockEvaluator = null) {
      var unit         = f.GetPointer<Unit>(unitEntity);
      var aimDirection = aimTarget - aimOrigin;
      var isBlocked    = aimBlockEvaluator?.Invoke(f, unitEntity, spectatorCamera, aimOrigin, aimTarget) ?? false;

#if DEBUG
      {
        var debugShotOriginOffset = unit->GetActiveWeaponConfig(f) is { } activeWeaponConfig
          ? activeWeaponConfig.shotOriginOffset
          : FPVector3.Zero;

        UnitHelper.DrawRig(f, unitEntity, this.characterTorsoOffset, this.characterShoulderOffset, debugShotOriginOffset);

        Draw.Line(aimTarget - aimDirection, aimTarget, ColorRGBA.Yellow);
      }
#endif

      unitAim->AimCurrentRotation         = FPQuaternionHelper.LookRotationAsYawPitchRoll(aimDirection, FPVector3.Up);
      unitAim->AimCurrentRotationVelocity = FPVector3.Zero;

      unitAim->AimCurrentPosition         = aimTarget;
      unitAim->AimCurrentPositionVelocity = FPVector3.Zero;

      unit->IsTargetBlocked = isBlocked;

      ScanTargetEnemy(f, unitEntity, unitAim);

      if (unit->IsTargetBlocked) {
        unit->HasTarget                    = false;
        unit->Target                       = EntityRef.None;
        unit->TargetPositionLagCompensated = FPVector3.Zero;
      }

      if (autoAimEnabled && unit->HasTarget) {
        var aimPoint = unit->TargetPositionLagCompensated;

        unitAim->AimCurrentPosition = aimPoint;
        unitAim->AimCurrentRotation = FPQuaternionHelper.LookRotationAsYawPitchRoll(
          aimPoint - aimOrigin,
          FPVector3.Up
        );
      }
    }
    protected void ScanTargetEnemy(Frame f, EntityRef unitEntity, UnitAim* unitAim) {
      var unit = f.GetPointer<Unit>(unitEntity);

      if (CharacterFsm.CurrentStateIs<CharacterStateSprint>(f, unitEntity)) {
        unit->HasTarget                    = false;
        unit->Target                       = EntityRef.None;
        unit->TargetPositionLagCompensated = FPVector3.Zero;
        return;
      }

      EntityRef weaponEntity = unit->ActiveWeaponRef;

      if (!weaponEntity.IsValid || unit->IsWeaponChanging) {
        unit->HasTarget                    = false;
        unit->Target                       = EntityRef.None;
        unit->TargetPositionLagCompensated = FPVector3.Zero;
        return;
      }

      var weapon = f.GetPointer<Weapon>(weaponEntity);
      var config = unit->GetActiveWeaponConfig(f)!;

      var range         = weapon->CurrentStats.attackDistance;
      var triggerAngleX = weapon->CurrentStats.triggerAngleX;
      var triggerAngleY = weapon->CurrentStats.triggerAngleY;

      var lookRotation  = FPQuaternionHelper.CreateFromYawPitchRoll(unitAim->AimCurrentRotation);
      var lookDirection = lookRotation * FPVector3.Forward;
      var shooterPos    = this.GetAimOrigin(f, unitEntity);

#if DEBUG && false
      {
        var fwd = FPVector3.Forward * range;

        var b = shooterPos + (lookRotation * FPQuaternion.Euler(triggerAngleY.AsFP, triggerAngleX.AsFP, 0)) * fwd;
        var c = shooterPos + (lookRotation * FPQuaternion.Euler(triggerAngleY.AsFP, -triggerAngleX.AsFP, 0)) * fwd;
        var d = shooterPos + (lookRotation * FPQuaternion.Euler(-triggerAngleY.AsFP, -triggerAngleX.AsFP, 0)) * fwd;
        var e = shooterPos + (lookRotation * FPQuaternion.Euler(-triggerAngleY.AsFP, triggerAngleX.AsFP, 0)) * fwd;

        // Draw.Line(b, c, ColorRGBA.Cyan);
        // Draw.Line(c, d, ColorRGBA.Cyan);
        // Draw.Line(d, e, ColorRGBA.Cyan);
        // Draw.Line(e, b, ColorRGBA.Cyan);

        // Draw.Line(shooterPos, b, ColorRGBA.Cyan);
        // Draw.Line(shooterPos, c, ColorRGBA.Cyan);
        // Draw.Line(shooterPos, d, ColorRGBA.Cyan);
        // Draw.Line(shooterPos, e, ColorRGBA.Cyan);
      }
#endif

      using (targetListPool.Get(out var targets)) {
        PhysicsHelper.RaycastShapeCollision(f,
          targets,
          unitEntity,
          shooterPos,
          lookDirection,
          range,
          new FPVector2(triggerAngleX, triggerAngleY),
          config.shotTargetOffsetY,
          targetFilter: static (frame, sourceRef, targetRef) => TargetFilter(frame, sourceRef, targetRef));

        unit->HasTarget = targets.Count > 0;

        if (!unit->HasTarget) {
          unit->Target                       = EntityRef.None;
          unit->TargetPositionLagCompensated = FPVector3.Zero;
          return;
        }

        var nearRange    = range * config.priorityAimDistanceCoefficient;
        var sqrNearRange = nearRange * nearRange;

        targets.Sort((a, b) => {
          var cmp = -a.Angle.CompareTo(b.Angle);

          return cmp != 0 ? cmp : a.EntityRef.Index.CompareTo(b.EntityRef.Index);
        });

        var target = targets[0];

        for (var i = 0; i < targets.Count; i++) {
          if (targets[i].SqrDistance <= sqrNearRange) {
            target = targets[i];
            break;
          }
        }

        unit->Target                       = target.EntityRef;
        unit->TargetPositionLagCompensated = target.EntityPositionLagCompensated;
      }
    }

    static bool TargetFilter(Frame f, EntityRef sourceRef, EntityRef targetRef) {
      if (sourceRef == targetRef) {
        return false;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, targetRef) || f.Has<UnitExited>(targetRef)) {
        return false;
      }

      if (f.TryGetPointer(targetRef, out Team* targetTeam)) {
        if (f.TryGetPointer(sourceRef, out Team* sourceTeam)) {
          if (sourceTeam->Equals(targetTeam)) {
            return false;
          }
        }
      }

      var sourceTransform = f.GetPointer<Transform3D>(sourceRef);
      var targetTransform = f.GetPointer<Transform3D>(targetRef);

      if (!LineOfSightHelper.HasLineSight(f, sourceTransform->Position + FPVector3.Up, targetTransform->Position + FPVector3.Up)) {
        return false;
      }

      return true;
    }
  }
}