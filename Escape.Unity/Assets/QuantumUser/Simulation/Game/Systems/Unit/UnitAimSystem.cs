namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class UnitAimSystem : SystemMainThreadFilter<UnitAimSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Transform3D* Transform;
      public Unit*        Unit;
      // public KCC*                      KCC;
      public UnitAim*                  UnitAim;
      public CharacterSpectatorCamera* SpectatorCamera;
      public Team*                     Team;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<BotInvisibleByPlayer>();

    public override void Update(Frame f, ref Filter filter) {     
      if (f.TryGetPointer(filter.Entity, out Turret* turret)) {
        if (filter.Unit->HasTarget) {
          var targetTransform = f.GetPointer<Transform3D>(filter.Unit->Target);
          var targetPosition = targetTransform->Position;
          
          var targetHeight = UnitColliderHeightHelper.GetCurrentHeight(f, filter.Unit->Target);
          if (targetHeight > FP._0) {
            targetPosition += FPVector3.Up * (targetHeight * FP._0_50);
          }
          
          filter.UnitAim->AimCurrentPosition = targetPosition;
        }
      }
      else if (f.IsVerified || f.Context.IsLocalPlayer(filter.Unit->PlayerRef)) {
        f.GameModeAiming.UpdateAim(f, filter.Entity, filter.UnitAim, filter.SpectatorCamera);
      }

      TransformHelper.SetPositionAndRotation(
        f.Unsafe.GetPointer<Transform3D>(filter.UnitAim->AimEntity),
        filter.UnitAim->AimCurrentPosition,
        FPQuaternionHelper.CreateFromYawPitchRoll(filter.UnitAim->AimCurrentRotation)
      );
    }
  }
}