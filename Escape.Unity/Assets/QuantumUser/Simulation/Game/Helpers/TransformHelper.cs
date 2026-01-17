using System;
using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum {
  public static unsafe class TransformHelper {
    public static void CopyPositionAndRotation(Frame f, EntityRef fromRef, EntityRef toRef) {
      var from = f.GetPointer<Transform3D>(fromRef);
      var to   = f.GetPointer<Transform3D>(toRef);

      CopyPositionAndRotation(from, to);
    }

    public static void CopyPositionAndRotation(Frame f, Transform3D* from, EntityRef toRef) {
      var to = f.GetPointer<Transform3D>(toRef);

      CopyPositionAndRotation(from, to);
    }

    public static void CopyPositionAndRotation(Transform3D* from, Transform3D* to) {
      to->Position = from->Position;
      to->Rotation = from->Rotation;
    }

    public static void CopyPositionAndRotation(KCC* from, Transform3D* to) {
      to->Position = from->Position;
      to->Rotation = from->Data.LookRotation;
    }

    public static void CopyRotation(Frame f, EntityRef fromRef, EntityRef toRef) {
      var from = f.GetPointer<Transform3D>(fromRef);
      var to   = f.GetPointer<Transform3D>(toRef);
      to->Rotation = from->Rotation;
    }

    public static void SetPositionAndRotation(Transform3D* to, FPVector3 position, FP rotationRad) {
      to->Position = position;
      to->Rotation = FPQuaternion.RadianAxis(rotationRad, FPVector3.Up);
    }

    public static void SetPositionAndRotation(Transform3D* to, FPVector3 position, FPQuaternion rotation) {
      to->Position = position;
      to->Rotation = rotation;
    }

    public static FP AngleRadiansToLookAtTarget(Transform2D* source, Transform2D* target) {
      return FPMath.Abs(AngleSignedRadiansToLookAtTarget(source, target));
    }

    public static FP AngleRadiansToLookAtTarget(Transform3D* source, Transform3D* target) {
      return FPMath.Abs(AngleSignedRadiansToLookAtTarget(source, target));
    }

    public static FP AngleRadiansToLookAtTarget(Transform2D* source, FPVector2 targetPosition) {
      var angle = AngleSignedRadiansToLookAtTarget(source->Position, source->Rotation, targetPosition);
      return FPMath.Abs(angle);
    }

    public static FP AngleSignedRadiansToLookAtTarget(Transform2D* source, Transform2D* target) {
      return AngleSignedRadiansToLookAtTarget(source->Position, source->Rotation, target->Position);
    }

    public static FP AngleSignedRadiansToLookAtTarget(Transform3D* source, Transform3D* target) {
      return AngleSignedRadiansToLookAtTarget(
              source->Position,
              source->Rotation,
              target->Position);
    }

    public static FP AngleSignedRadiansToLookAtTarget(Transform2D* source, FPVector2 target) {
      return AngleSignedRadiansToLookAtTarget(source->Position, source->Rotation, target);
    }

    public static FP AngleSignedRadiansToLookAtTarget(FPVector2 sourcePosition, FP sourceRotation, FPVector2 targetPosition) {
      FPVector2 directionToTarget = (targetPosition - sourcePosition).Normalized;

      // Находим угол поворота к цели.
      // целевой угол смещён на -Pi/2.
      FP targetRotation = FPMath.Atan2(directionToTarget.Y, directionToTarget.X) - FP.PiOver2;

      FP angleToTarget = FPMath.AngleBetweenRadians(sourceRotation, targetRotation);

      return angleToTarget;
    }

    public static FP AngleSignedRadiansToLookAtTarget(FPVector3 sourcePosition, FPQuaternion sourceRotation, FPVector3 targetPosition) {
      // 1) Берём вектор до цели только в XZ
      FPVector3 directionToTarget = targetPosition - sourcePosition;
      directionToTarget.Y = FP._0;
      directionToTarget   = directionToTarget.Normalized;

      // 2) Получаем текущий yaw из кватерниона (в градусах)
      FPVector3 eulerDeg = sourceRotation.AsEuler;
      // Берём Y, переводим в радианы
      FP currentYawRad = eulerDeg.Y * FP.Deg2Rad;

      // 3) Угол (yaw) к цели (0 вдоль +Z)
      FP targetYawRad = FPMath.Atan2(directionToTarget.X, directionToTarget.Z);

      // 4) Разница углов ([-π..π])
      FP angleToTarget = FPMath.AngleBetweenRadians(currentYawRad, targetYawRad);

      return angleToTarget;
    }

    public static void LookAtTarget(Transform3D* source, Transform3D* target, FP deltaTime, FP maxAngleDegrees) {
      FP angleToTarget = AngleSignedRadiansToLookAtTarget(source, target);

      maxAngleDegrees *= FP.Deg2Rad;

      FP minAngle = FPMath.Min(
              FPMath.Abs(angleToTarget),
              maxAngleDegrees * deltaTime);

      // Берём шаг поворота, ограниченный макс. углом в секунду
      FP angleStep = minAngle * FPMath.Sign(angleToTarget);

      // Создаём "приращение" (небольшой кватернион) вокруг глобальной Y
      FPQuaternion deltaRotation = FPQuaternion.AngleAxis(angleStep * FP.Rad2Deg, FPVector3.Up);

      // Умножаем: сначала deltaRotation, потом старая ориентация
      // (это даёт поворот вокруг МИРОВОЙ оси Y)
      source->Rotation = deltaRotation * source->Rotation;
    }

    public static void LookAtTarget(Transform2D* source, Transform2D* target, FP deltaTime, FP maxAngleRadians) {
      FP angleToTarget = AngleSignedRadiansToLookAtTarget(source, target);

      source->Rotation += FPMath.Min(FPMath.Abs(angleToTarget), maxAngleRadians * deltaTime) * FPMath.Sign(angleToTarget);
    }

    public static bool IsLookAtTarget(Transform2D* source, Transform2D* target, FP thresholdAngle) {
      FP angleToTarget = AngleRadiansToLookAtTarget(source, target);

      return angleToTarget < thresholdAngle * FP.Deg2Rad;
    }

    public static bool IsLookAtTarget(Transform3D* source, Transform3D* target, FP thresholdAngle) {
      FP angleToTarget = AngleRadiansToLookAtTarget(source, target);

      return angleToTarget < thresholdAngle * FP.Deg2Rad;
    }

    public static FP Distance(Transform2D* source, Transform2D* target) =>
            FPVector2.Distance(source->Position, target->Position);

    public static FP Distance(Transform3D* source, Transform3D* target) =>
            FPVector3.Distance(source->Position, target->Position);

    public static FP Distance(Frame f, EntityRef source, EntityRef target) =>
            FPVector3.Distance(f.Get<Transform3D>(source).Position, f.Get<Transform3D>(target).Position);

    public static FP DistanceSquared(Transform2D* source, Transform2D* target) =>
            FPVector2.DistanceSquared(source->Position, target->Position);

    public static FP DistanceSquared(Transform2D source, Transform2D target) =>
            FPVector2.DistanceSquared(source.Position, target.Position);

    public static FP DistanceSquared(Transform3D source, Transform3D target) =>
            FPVector3.DistanceSquared(source.Position, target.Position);

    public static FP DistanceSquared(Transform3D* source, Transform3D* target) =>
            FPVector3.DistanceSquared(source->Position, target->Position);

    public static bool TryFindClosest<T>(this EntityRef sourceRef, Frame f,
            out EntityRef closestRef) where T : unmanaged, IComponent {

// #if TRACE
//     using (new StopwatchBlock(nameof(TryFindClosest)));
// #endif

      closestRef = EntityRef.None;
      FP min = FP.MaxValue;

      var filter = f.Filter<T, Transform3D>();
      var source = f.GetPointer<Transform3D>(sourceRef);

      while (filter.NextUnsafe(out EntityRef e, out _, out Transform3D* other)) {
        FP sqrDist = DistanceSquared(source, other);
        if (sqrDist < min) {
          min        = sqrDist;
          closestRef = e;
        }
      }

      return closestRef != EntityRef.None;
    }

    public static bool TryFindClosest<T>(this EntityRef sourceRef, Frame f,
            out EntityRef closestRef,
            out Transform3D closestTransform)
            where T : unmanaged, IComponent {

// #if TRACE
//     using (new StopwatchBlock(nameof(TryFindClosest)));
// #endif

      closestRef       = EntityRef.None;
      closestTransform = default;

      var source = f.GetPointer<Transform3D>(sourceRef);
      var filter = f.Filter<T, Transform3D>();

      FP min = FP.MaxValue;
      while (filter.NextUnsafe(out EntityRef e, out _, out Transform3D* other)) {
        FP sqrDist = DistanceSquared(source, other);
        if (sqrDist < min) {
          min              = sqrDist;
          closestRef       = e;
          closestTransform = *other;
        }
      }

      return closestRef != EntityRef.None;
    }

    public static bool TryFindClosest<T>(this EntityRef sourceRef, Frame f, Func<T, bool> predicate,
            out EntityRef closest) where T : unmanaged, IComponent {

// #if TRACE
//     using (new StopwatchBlock(nameof(TryFindClosest)));
// #endif

      closest = EntityRef.None;
      var min = FP.MaxValue;

      var filter = f.Filter<T, Transform3D>();
      var source = f.Get<Transform3D>(sourceRef);

      while (filter.Next(out var e, out T t, out var other)) {
        if (predicate(t)) {
          var sqrDist = DistanceSquared(source, other);
          if (sqrDist < min) {
            min     = sqrDist;
            closest = e;
          }
        }
      }

      return closest != EntityRef.None;
    }

    public static bool TryFindFarthest<T>(this EntityRef sourceRef, Frame f,
            out EntityRef farthest) where T : unmanaged, IComponent {

// #if TRACE
//     using (new StopwatchBlock(nameof(TryFindFarthest)));
// #endif

      farthest = EntityRef.None;
      FP max = FP.Minus_1;

      var filter = f.Filter<T, Transform3D>();
      var source = f.GetPointer<Transform3D>(sourceRef);

      while (filter.NextUnsafe(out EntityRef e, out _, out Transform3D* other)) {
        FP sqrDist = DistanceSquared(source, other);
        if (sqrDist > max) {
          max      = sqrDist;
          farthest = e;
        }
      }

      return farthest != EntityRef.None;
    }

    public static bool TryFindInRange<T>(Frame f, Transform3D* origin, FP squaredRange,
            out EntityRef inRange) where T : unmanaged, IComponent {

// #if TRACE
//     using (new StopwatchBlock(nameof(TryFindInRange)));
// #endif

      var filter = f.Filter<Transform3D, T>();
      while (filter.NextUnsafe(out EntityRef e, out Transform3D* other, out T* c)) {
        if (DistanceSquared(origin, other) < squaredRange) {
          inRange = e;
          return true;
        }
      }

      inRange = EntityRef.None;
      return false;
    }

    public static bool TryFindRandom<T>(Frame f, RNGSession* rng,
            out EntityRef result) where T : unmanaged, IComponent {

// #if TRACE
//     using (new StopwatchBlock(nameof(TryFindRandom)));
// #endif

      result = EntityRef.None;

      var filter   = f.Filter<T, Transform3D>();
      var entities = new List<EntityRef>();

      while (filter.NextUnsafe(out EntityRef entity, out _, out Transform3D* other)) {
        entities.Add(entity);
      }

      if (entities.Count > 0) {
        int index = rng->Next(0, entities.Count);
        result = entities[index];

        return true;
      }

      return false;
    }
  }
}