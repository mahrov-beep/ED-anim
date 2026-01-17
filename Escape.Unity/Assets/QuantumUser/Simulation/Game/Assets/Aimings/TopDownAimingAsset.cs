namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  [Serializable]
  public unsafe class TopDownAimingAsset : AimingAsset {
    public override bool UseTopDownQuantumPredictionCulling => true;

    public override void UpdateSpectatorCamera(Frame f, EntityRef unitEntity, InputContainer* input,
      CharacterSpectatorCamera* spectatorCamera) {
      var unit = f.GetPointer<Unit>(unitEntity);

      var prevCameraDesiredRotation = spectatorCamera->SpectatorCameraDesiredRotation;
      var nextCameraDesiredRotation = prevCameraDesiredRotation + FPYawPitchRoll.CreateYaw(
        input->Input.LookRotationDelta.X * unit->CurrentStats.rotationSpeed);

      spectatorCamera->SpectatorCameraDesiredRotation = nextCameraDesiredRotation;
      spectatorCamera->SpectatorCameraCurrentRotation = FPYawPitchRollHelper.SmoothDampUnclamped(
        current: spectatorCamera->SpectatorCameraCurrentRotation,
        target: nextCameraDesiredRotation,
        currentVelocity: ref spectatorCamera->SpectatorCameraCurrentVelocity,
        smoothTime: (FP)85 / 1000, // скорость поворота камеры
        deltaTime: f.DeltaTime
      );
    }

    public override void UpdateAim(Frame f, EntityRef unitEntity, UnitAim* unitAim, CharacterSpectatorCamera* spectatorCamera) {
      //
      // для режима TopDown делаем плавающий прицел, то есть при вращении камеры
      // направление стрельбы плавно поворачивается в сторону куда смотрит камера
      //
      unitAim->AimCurrentRotation = FPYawPitchRollHelper.SmoothDampUnclamped(
        current: unitAim->AimCurrentRotation,
        target: spectatorCamera->SpectatorCameraDesiredRotation,
        currentVelocity: ref unitAim->AimCurrentRotationVelocity,
        smoothTime: (FP)70 / 1000, // скорость поворота прицела
        deltaTime: f.DeltaTime
      );

      unitAim->AimCurrentPosition = FPVector3Helper.SmoothDampUnclamped(
        current: unitAim->AimCurrentPosition,
        target: GetAimDesiredPosition(f, unitEntity, unitAim),
        currentVelocity: ref unitAim->AimCurrentPositionVelocity,
        smoothTime: (FP)20 / 1000,
        deltaTime: f.DeltaTime
      );
    }

    public override void ApplySpectatorEntity(Frame f, EntityRef unitEntity, CharacterSpectatorCamera* spectatorCamera) {
      var spectatorTransform = f.GetPointer<Transform3D>(spectatorCamera->CameraEntity);
      var unitPosition       = UnitHelper.GetPosition(f, unitEntity);

      TransformHelper.SetPositionAndRotation(
        spectatorTransform,
        unitPosition,
        FPQuaternionHelper.CreateFromYawPitchRoll(spectatorCamera->SpectatorCameraCurrentRotation)
      );
    }

    FPVector3 GetAimDesiredPosition(Frame f, EntityRef unitEntity, UnitAim* unitAim) {
      var unit = f.GetPointer<Unit>(unitEntity);

      var unitPosition = UnitHelper.GetPosition(f, unitEntity);
      var weaponEntity = unit->ActiveWeaponRef;

      if (!weaponEntity.IsValid || unit->IsWeaponChanging) {
        return unitPosition;
      }

      ScanTargetEnemy(f, unitEntity, unitAim);

      var weapon       = f.GetPointer<Weapon>(weaponEntity);
      var weaponConfig = weapon->GetConfig(f);

      var maxRange     = weapon->CurrentStats.attackDistance.AsFP;
      var targetHeight = unit->HasTarget
              ? UnitColliderHeightHelper.GetCurrentHeight(f, unit->Target)
              : UnitColliderHeightHelper.GetCurrentHeight(f, unitEntity);
      var targetOffset = targetHeight > FP._0 ? FPVector3.Up * (targetHeight * FP._0_50) : FPVector3.Zero;

      if (unit->HasTarget) {
        var targetPosition = UnitHelper.GetPosition(f, unit->Target);
        return targetPosition + targetOffset;
      }

      var lookRotation = FPQuaternionHelper.CreateFromYawPitchRoll(unitAim->AimCurrentRotation);
      return unitPosition + lookRotation * (FPVector3.Forward * maxRange) + targetOffset;
    }
  }
}