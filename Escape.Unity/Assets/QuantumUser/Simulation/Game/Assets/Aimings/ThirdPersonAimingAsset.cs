namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;
  using UnityEngine.Serialization;

  [Serializable]
  public unsafe class ThirdPersonAimingAsset : AimingAsset {
    [RangeEx(-80, 0)] public FP minLookRotationPitch = -45;
    [RangeEx(0, 80)]  public FP maxLookRotationPitch = 60;

    public bool autoAim;

    public override bool UseTopDownQuantumPredictionCulling => false;

    public override void UpdateSpectatorCamera(Frame f, EntityRef unitEntity, InputContainer* input,
            CharacterSpectatorCamera* spectatorCamera) {

      var unit = f.GetPointer<Unit>(unitEntity);

      var prevCameraDesiredRotation = spectatorCamera->SpectatorCameraDesiredRotation;
      var nextCameraDesiredRotation = prevCameraDesiredRotation +
              FPYawPitchRoll.Create(input->Input.LookRotationDelta.XYO);

      nextCameraDesiredRotation.Pitch = FPMath.Clamp(
              nextCameraDesiredRotation.Pitch,
              -maxLookRotationPitch * FP.Deg2Rad,
              -minLookRotationPitch * FP.Deg2Rad);

      spectatorCamera->SpectatorCameraDesiredRotation = nextCameraDesiredRotation;
      spectatorCamera->SpectatorCameraCurrentRotation = nextCameraDesiredRotation;

      spectatorCamera->AimTarget = input->Input.AimTarget;
    }

    public override void UpdateAim(Frame f, EntityRef unitEntity, UnitAim* unitAim,
            CharacterSpectatorCamera* spectatorCamera) {
      var aimOrigin = GetAimOrigin(f, unitEntity);
      var aimTarget = spectatorCamera->AimTarget;

      UpdateStandardAim(f,
        unitEntity,
        unitAim,
        spectatorCamera,
        aimOrigin,
        aimTarget,
        autoAim,
        ThirdPersonAimBlockEvaluator);
    }

    public override void ApplySpectatorEntity(Frame f, EntityRef unitEntity, CharacterSpectatorCamera* spectatorCamera) {
      var spectatorTransform = f.GetPointer<Transform3D>(spectatorCamera->CameraEntity);

      TransformHelper.SetPositionAndRotation(
              spectatorTransform,
              GetAimOrigin(f, unitEntity),
              FPQuaternionHelper.CreateFromYawPitchRoll(spectatorCamera->SpectatorCameraCurrentRotation)
      );
    }
  
    static bool ThirdPersonAimBlockEvaluator(Frame frame, EntityRef _, CharacterSpectatorCamera* camera, FPVector3 origin, FPVector3 target) {
      var forwardRotation   = FPQuaternionHelper.CreateFromYawPitchRoll(camera->SpectatorCameraCurrentRotation);
      var inputAimVector    = target - origin;
      var inputAimDirection = inputAimVector.Normalized;
      var forwardDirection  = forwardRotation * FPVector3.Forward;

      if (FPVector3.Dot(inputAimDirection, forwardDirection) < FP._1 - FP._0_05) {
        return true;
      }

      if (inputAimVector.Magnitude < FP._1) {
        return true;
      }

      return false;
    }
  }
}