namespace Quantum {
  using Core;
  using Photon.Deterministic;
  using Physics3D;

  public static unsafe class UnitColliderHeightHelper {
    public static void ApplyHeight(Frame f, EntityRef entity, FP requestedRatio) {
      var ratio = NormalizeRatio(requestedRatio);

      if (!TryResolve(f, entity, out var collider, out var baseHeight)) {
        return;
      }

      AdjustColliderShape(collider, baseHeight, ratio);
    }

    public static void ResetHeight(Frame f, EntityRef entity) {
      ApplyHeight(f, entity, FP._1);
    }

    public static FP GetCurrentHeightRatio(Frame f, EntityRef entity) {
      return TryGetHeights(f, entity, out _, out var baseHeight, out var currentHeight)
              ? ComputeCurrentRatio(currentHeight, baseHeight)
              : FP._1;
    }

    public static FP GetCurrentHeightRatio(FrameThreadSafe f, EntityRef entity) {
      return TryGetHeights(f, entity, out _, out var baseHeight, out var currentHeight)
              ? ComputeCurrentRatio(currentHeight, baseHeight)
              : FP._1;
    }

    public static FP GetCurrentHeight(FrameBase f, EntityRef entity) {
      return TryGetHeights(f, entity, out _, out _, out var currentHeight) ? currentHeight : FP._0;
    }

    public static FP GetCurrentHeight(FrameThreadSafe f, EntityRef entity) {
      return TryGetHeights(f, entity, out _, out _, out var currentHeight) ? currentHeight : FP._0;
    }

    static bool TryResolve(FrameBase f, EntityRef entity, out PhysicsCollider3D* collider, out FP baseHeight) {
      collider   = null;
      baseHeight = FP._0;

      if (!f.TryGetPointer(entity, out collider)) {
        return false;
      }

      if (!f.TryGetPointer(entity, out KCC* kcc)) {
        return false;
      }

      if (!f.TryFindAsset(kcc->Settings, out KCCSettings settings)) {
        return false;
      }

      baseHeight = settings.Height;
      return baseHeight > FP._0;
    }

    static bool TryResolve(FrameThreadSafe f, EntityRef entity, out PhysicsCollider3D* collider, out FP baseHeight) {
      collider   = null;
      baseHeight = FP._0;

      if (!f.TryGetPointer(entity, out collider)) {
        return false;
      }

      if (!f.TryGetPointer(entity, out KCC* kcc)) {
        return false;
      }

      var settings = f.FindAsset(kcc->Settings);
      if (settings == null) {
        return false;
      }

      baseHeight = settings.Height;
      return baseHeight > FP._0;
    }

    static bool TryGetHeights(FrameBase f, EntityRef entity, out PhysicsCollider3D* collider, out FP baseHeight, out FP currentHeight) {
      if (!TryResolve(f, entity, out collider, out baseHeight)) {
        currentHeight = FP._0;
        return false;
      }

      currentHeight = ComputeCurrentHeight(collider);
      return currentHeight > FP._0;
    }

    static bool TryGetHeights(FrameThreadSafe f, EntityRef entity, out PhysicsCollider3D* collider, out FP baseHeight, out FP currentHeight) {
      if (!TryResolve(f, entity, out collider, out baseHeight)) {
        currentHeight = FP._0;
        return false;
      }

      currentHeight = ComputeCurrentHeight(collider);
      return currentHeight > FP._0;
    }

    static FP NormalizeRatio(FP ratio) {
      if (ratio <= FP._0) {
        return FP._1;
      }

      return FPMath.Clamp(ratio, FP._0_10, FP._1);
    }

    static void AdjustColliderShape(PhysicsCollider3D* collider, FP baseHeight, FP ratio) {
      if (collider == null || baseHeight <= FP._0) {
        return;
      }

      var shape = collider->Shape;
      if (shape.Type != Shape3DType.Capsule) {
        return;
      }

      var capsule = shape.Capsule;
      FP radius   = capsule.Radius;
      if (radius <= FP._0) {
        return;
      }

      FP currentExtent  = capsule.Extent;
      FP currentHeight  = (radius + currentExtent) * FP._2;
      FP baselineOffset = shape.LocalTransform.Position.Y + (baseHeight - currentHeight) * FP._0_50;

      FP targetHeight = baseHeight * ratio;
      if (targetHeight <= FP._0) {
        return;
      }

      FP targetExtent = FPMath.Max(FP._0, targetHeight * FP._0_50 - radius);
      FP newOffsetY   = baselineOffset - (baseHeight - targetHeight) * FP._0_50;

      if (capsule.Extent == targetExtent && shape.LocalTransform.Position.Y == newOffsetY) {
        return;
      }

      capsule.Extent = targetExtent;
      shape.Capsule  = capsule;

      var localTransform      = shape.LocalTransform;
      localTransform.Position = new FPVector3(localTransform.Position.X, newOffsetY, localTransform.Position.Z);
      shape.LocalTransform    = localTransform;

      collider->Shape = shape;
    }

    static FP ComputeCurrentHeight(PhysicsCollider3D* collider) {
      if (collider == null) {
        return FP._0;
      }

      var shape = collider->Shape;
      if (shape.Type != Shape3DType.Capsule) {
        return FP._0;
      }

      var capsule      = shape.Capsule;
      var radius       = capsule.Radius;
      return (radius + capsule.Extent) * FP._2;
    }

    static FP ComputeCurrentRatio(FP currentHeight, FP baseHeight) {
      if (currentHeight <= FP._0 || baseHeight <= FP._0) {
        return FP._1;
      }

      return NormalizeRatio(currentHeight / baseHeight);
    }
  }
}
