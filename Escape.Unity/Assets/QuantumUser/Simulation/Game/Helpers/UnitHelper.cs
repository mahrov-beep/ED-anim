namespace Quantum {
  using System.Diagnostics;
  using Photon.Deterministic;
  using Debug = UnityEngine.Debug;

  public static unsafe class UnitHelper {
    public static FPVector3 GetPosition(Frame f, EntityRef unitEntity) {
      return f.TryGetPointer(unitEntity, out KCC* kcc)
        ? kcc->Position
        : f.Get<Transform3D>(unitEntity).Position;
    }

    public static Transform3D GetTransform(Frame f, EntityRef unitEntity) {
      return f.TryGetPointer(unitEntity, out KCC* kcc)
        ? Transform3D.Create(kcc->Position, FPQuaternion.CreateFromYawPitchRoll(kcc->Data.LookYaw * FP.Deg2Rad, FP._0, FP._0))
        : f.Get<Transform3D>(unitEntity);
    }


    public static void CalculateRig(Frame f, EntityRef unitEntity, FPVector3 torso, FPVector3 shoulder, FPVector3 shotOrigin,
      out FPVector3 calculatedTorso, out FPVector3 calculatedShoulder, out FPVector3 calculatedShotOrigin) {
      var position = GetPosition(f, unitEntity);
      var aimLook  = FPQuaternionHelper.CreateFromYawPitchRoll(f.GetPointer<UnitAim>(unitEntity)->AimCurrentRotation);

      var heightRatio          = UnitColliderHeightHelper.GetCurrentHeightRatio(f, unitEntity);
      var scaledTorsoOffset    = ApplyHeightRatio(torso, heightRatio);
      var scaledShoulderOffset = ApplyHeightRatio(shoulder, heightRatio);
      var scaledShotOffset     = ApplyHeightRatio(shotOrigin, heightRatio);

      calculatedTorso      = position + scaledTorsoOffset;
      calculatedShoulder   = calculatedTorso + aimLook * (scaledShoulderOffset - scaledTorsoOffset);
      calculatedShotOrigin = calculatedShoulder + aimLook * (scaledShotOffset - scaledShoulderOffset);
    }

    static FPVector3 ApplyHeightRatio(FPVector3 offset, FP ratio) {
      if (ratio <= FP._0 || ratio == FP._1) {
        return offset;
      }

      return new FPVector3(offset.X, offset.Y * ratio, offset.Z);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawRig(Frame f, EntityRef unitEntity, FPVector3 torso, FPVector3 shoulder, FPVector3 shotOrigin) {
      var position = GetPosition(f, unitEntity);
      CalculateRig(f, unitEntity, torso, shoulder, shotOrigin, out var worldTorso, out var worldShoulder, out var worldShotOrigin);

      // Draw.Line(position, worldTorso, ColorRGBA.Yellow);
      // Draw.Line(worldTorso, worldShoulder, ColorRGBA.Yellow);
      // Draw.Line(worldShoulder, worldShotOrigin, ColorRGBA.Yellow);

      // Draw.Sphere(worldTorso, FP._0_05, ColorRGBA.Yellow);
      // Draw.Sphere(worldShoulder, FP._0_04, ColorRGBA.Yellow);
      // Draw.Sphere(worldShotOrigin, FP._0_03, ColorRGBA.Yellow);
    }
  }
}