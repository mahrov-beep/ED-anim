using Photon.Deterministic;

namespace Quantum {
  public static unsafe class LineOfSightHelper {
    public static bool HasLineSight(Frame f, EntityRef source, EntityRef target) {
      bool has = HasLineSight(f,
              f.GetPointer<Transform3D>(source)->Position,
              f.GetPointer<Transform3D>(target)->Position);

      return has;
    }

    public static bool HasLineSight(Frame f, Transform3D* source, Transform3D* target) {
      bool has = HasLineSight(f, source->Position, target->Position);
      return has;
    }

    public static bool HasLineSight(Frame f, FPVector3 source, FPVector3 target) {
      var layerMask = PhysicsHelper.GetStaticLayerMask(f);

      Physics3D.HitCollection3D hits = f.Physics3D.LinecastAll(source, target, layerMask, QueryOptions.HitAll);
      for (var i = 0; i < hits.Count; i++) {
        if (hits[i].IsDynamic == false) {
          return false;
        }
      }
      return true;
    }

    public static bool HasLineSightFast(Frame f, EntityRef sourceRef, EntityRef targetRef) {
      var layerMask = PhysicsHelper.GetBlockRaycastLayerMask(f);

      if (!f.TryGetPointer(targetRef, out Transform3D* target)) {
        f.LogError(sourceRef, $"Has not transform on target entity ({targetRef})");
        return false;
      }

      if (!f.TryGetPointer(sourceRef, out Transform3D* source)) {
        f.LogError(sourceRef, $"Has not transform");
        return false;
      }

      var sourceHeight = UnitColliderHeightHelper.GetCurrentHeight(f, sourceRef);
      var targetHeight = UnitColliderHeightHelper.GetCurrentHeight(f, targetRef);

      var sourceEyePos = source->Position + FPVector3.Up * sourceHeight;
      var targetEyePos = target->Position + FPVector3.Up * targetHeight;

      var directionOffset = FP._0_05 * (targetEyePos - sourceEyePos);

      var hit = f.Physics3D.Linecast(
              sourceEyePos,
              targetEyePos + directionOffset,
              layerMask,
              QueryOptions.HitStatics |
              QueryOptions.HitKinematics |
              QueryOptions.HitDynamics);

      return hit.HasValue && hit.Value.Entity == targetRef;
    }

    public static bool HasLineSightFast(Frame f, FPVector3 source, EntityRef targetRef) {
      var layerMask = PhysicsHelper.GetBlockRaycastLayerMask(f);
      
      if (!f.TryGetPointer(targetRef, out Transform3D* target)) {
        f.LogError(EntityRef.None, $"Has not transform on target entity ({targetRef})");
        return false;
      }
      
      var hit = f.Physics3D.Linecast(
              source,
              target->Position,
              layerMask,
              QueryOptions.HitStatics |
              QueryOptions.HitKinematics |
              QueryOptions.HitDynamics);

      return hit.HasValue && hit.Value.Entity == targetRef;
    }

    // public static bool HasLineSight(FrameThreadSafe f, Transform3D* source, Transform3D* target) {
    //   bool has = HasLineSight(f, source->Position, target->Position);
    //   return has;
    // }
    //
    // public static bool HasLineSight(FrameThreadSafe f, FPVector3 source, FPVector3 target) {
    //   var layerMask = PhysicsHelper.GetStaticLayerMask(f);
    //
    //   Physics3D.HitCollection3D hits = f.Physics3D.LinecastAll(source, target, layerMask, QueryOptions.HitStatics);
    //   for (var i = 0; i < hits.Count; i++) {
    //     if (hits[i].IsDynamic == false) {
    //       return false;
    //     }
    //   }
    //   return true;
    // }

    public static bool AnyStaticBetween(FrameThreadSafe f, FPVector3 from, FPVector3 to) {
      var layerMask = PhysicsHelper.GetStaticLayerMask(f);

      var hit = f.Physics3D.Linecast(from, to, layerMask, QueryOptions.HitStatics);

      return hit is { IsStatic: true };
    }
    
    public static bool AnyStaticBetween(Frame f, FPVector3 from, FPVector3 to) {
      var layerMask = PhysicsHelper.GetStaticLayerMask(f);

      var hit = f.Physics3D.Linecast(from, to, layerMask, QueryOptions.HitStatics);

      return hit is { IsStatic: true };
    }

    public static bool HasLineSight(Frame f, Transform2D* sourceTransform, Transform2D* targetTransform) {
      bool has = HasLineSight(f, sourceTransform->Position, targetTransform->Position);
      return has;
    }

    public static bool HasLineSight(Frame f, FPVector2 source, FPVector2 target) {
      var layerMask = PhysicsHelper.GetStaticLayerMask(f);

      Physics2D.HitCollection hits = f.Physics2D.LinecastAll(source, target, layerMask, QueryOptions.HitStatics);
      for (var i = 0; i < hits.Count; i++) {
        if (hits[i].IsDynamic == false) {
          return false;
        }
      }
      return true;
    }
  }
}