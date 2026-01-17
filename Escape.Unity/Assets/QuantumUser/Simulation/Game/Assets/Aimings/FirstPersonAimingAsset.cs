namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;
  
  [Serializable]
  public unsafe class FirstPersonAimingAsset : AimingAsset { 
    [RangeEx(-80, 0)] public FP minLookRotationPitch = -45;
    [RangeEx(0, 80)]  public FP maxLookRotationPitch = 60;

    public bool autoAim;

    public override bool UseTopDownQuantumPredictionCulling => false;

    public override void UpdateSpectatorCamera(Frame f, EntityRef unitEntity, InputContainer* input,
      CharacterSpectatorCamera* spectatorCamera)
    {
      var prevCameraDesiredRotation = spectatorCamera->SpectatorCameraDesiredRotation;
      var lookDelta                 = input->Input.LookRotationDelta.XYO;

      var filteredDelta = FPYawPitchRoll.Create(lookDelta);

      var nextCameraDesiredRotation = prevCameraDesiredRotation + filteredDelta;

      nextCameraDesiredRotation.Pitch = FPMath.Clamp(
        nextCameraDesiredRotation.Pitch,
        minLookRotationPitch * FP.Deg2Rad,
        maxLookRotationPitch * FP.Deg2Rad);

      spectatorCamera->SpectatorCameraDesiredRotation = nextCameraDesiredRotation;
      spectatorCamera->SpectatorCameraCurrentRotation = nextCameraDesiredRotation;

      spectatorCamera->AimTarget = input->Input.AimTarget;
    }

    public override FP GetCharacterRotation(Frame f, CharacterSpectatorCamera* spectatorCamera) {
      return spectatorCamera->SpectatorCameraCurrentRotation.Yaw;
    }

    public override void UpdateAim(Frame f, EntityRef unitEntity, UnitAim* unitAim,
            CharacterSpectatorCamera* spectatorCamera) {
      var aimOrigin = GetAimOrigin(f, unitEntity);
      var aimTarget = spectatorCamera->AimTarget;

      UpdateStandardAim(f, unitEntity, unitAim, spectatorCamera, aimOrigin, aimTarget, autoAim);
    }

    public override void ApplySpectatorEntity(Frame f, EntityRef unitEntity, CharacterSpectatorCamera* spectatorCamera) {
      var spectatorTransform = f.GetPointer<Transform3D>(spectatorCamera->CameraEntity);

      TransformHelper.SetPositionAndRotation(
              spectatorTransform,
              GetAimOrigin(f, unitEntity),
              FPQuaternionHelper.CreateFromYawPitchRoll(spectatorCamera->SpectatorCameraCurrentRotation)
      );
    }
  }
}