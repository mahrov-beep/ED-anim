namespace Quantum {
  using Photon.Deterministic;
  using static Photon.Deterministic.FPVector2;

  public unsafe class BotCameraRotationSystem : SystemMainThreadFilter<BotCameraRotationSystem.Filter> {
    public struct Filter {
      public EntityRef                 EntityRef;
      public Bot*                      Bot;
      public InputContainer*           InputContainer;
      public CharacterSpectatorCamera* SpectatorCamera;
      public Transform3D*              Transform;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, filter.EntityRef)) {
        return;
      }

      if (TryGetAttackTargetPosition(f, in filter, out var targetPos)) {
        UpdateRotationToTarget(f, in filter, targetPos);
        return;
      }

      if (filter.InputContainer->DesiredDirection != Zero) {
        UpdateRotationToMovementDirection(f, in filter);
      }
    }

    void UpdateRotationToTarget(Frame f, in Filter filter, FPVector3 targetPos) {
      var dirToTarget = (targetPos - filter.Transform->Position).XZ.Normalized;
      if (dirToTarget == Zero) {
        return;
      }

      var desiredAngle = RadiansSigned(dirToTarget, Up);
      filter.SpectatorCamera->SpectatorCameraDesiredRotation.Yaw = NormalizeYawAngle(
        filter.SpectatorCamera->SpectatorCameraDesiredRotation.Yaw,
        desiredAngle);

      filter.InputContainer->Input.AimTarget = targetPos;
    }

    void UpdateRotationToMovementDirection(Frame f, in Filter filter) {
      var desiredAngle = RadiansSigned(filter.InputContainer->DesiredDirection, Up);
      filter.SpectatorCamera->SpectatorCameraDesiredRotation.Yaw = NormalizeYawAngle(
        filter.SpectatorCamera->SpectatorCameraDesiredRotation.Yaw,
        desiredAngle);

      if (f.GameModeAiming is FirstPersonAimingAsset) {
        var aimOrigin       = f.GameModeAiming.GetAimOrigin(f, filter.EntityRef);
        var forwardRotation = FPQuaternionHelper.CreateFromYawPitchRoll(filter.SpectatorCamera->SpectatorCameraCurrentRotation);
        filter.InputContainer->Input.AimTarget = aimOrigin + forwardRotation * (FPVector3.Forward * FP._10);
      }
      else {
        filter.InputContainer->Input.AimTarget = filter.Transform->Position + filter.InputContainer->DesiredDirection.XOY * FP._10;
      }
    }

    static FP NormalizeYawAngle(FP currentYaw, FP targetYaw) {
      var delta = targetYaw - currentYaw;
      delta = FPMath.Repeat(delta + FP.Pi, FP.PiTimes2) - FP.Pi;
      return currentYaw + delta;
    }

    static bool TryGetAttackTargetPosition(Frame f, in Filter filter, out FPVector3 position) {
      position = default;
      var attackTarget = filter.Bot->Intent.AttackTarget;

      if (attackTarget == EntityRef.None) {
        return false;
      }

      var visibleEnemies = f.ResolveList(filter.Bot->VisibleEnemies);
      if (visibleEnemies.Contains(attackTarget) && f.TryGetPointer<Transform3D>(attackTarget, out var targetTransform)) {
        position = targetTransform->Position;
        var targetHeight = UnitColliderHeightHelper.GetCurrentHeight(f, attackTarget);
        if (targetHeight > FP._0) {
          position.Y += targetHeight * FP._0_50;
        }
        return true;
      }

      return false;
    }
  }
}
