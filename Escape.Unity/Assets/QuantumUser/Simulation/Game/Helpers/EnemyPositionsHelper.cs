using Photon.Deterministic;
using static Quantum.LineOfSightHelper;

namespace Quantum {
  using UnityEngine;
  public static unsafe class EnemyPositionsHelper {
    static readonly FP Angle360 = FP.PiTimes2 * FP.Rad2Deg;

    /*/// <summary>
    /// Ближайший враг которого видит тима юнита
    /// </summary>
    public static bool TryGetClosestEnemyVisibleOnMap(Frame f, EntityRef unit,
            out EntityRef closest, bool inUnitWeaponRange = false) {
      const bool lineSight = false;
      const bool checkFOW  = true;
      const bool enemyOnly = true;

      FP distanceLim = FP.UseableMax;
      if (inUnitWeaponRange) {
        // if (f.TryGetPointer<WeaponInventory>(unit, out var inventory)) {
        //   distanceLim = inventory->GetCurrentWeaponConfig(f).ShootDistance;
        // }
      }

      return TryGetClosestCharacter(f, unit, distanceLim, lineSight, checkFOW, enemyOnly,
              out closest);
    }*/

    /// <summary>
    /// Ближайший враг которого видит юнит в тумане войны по прямой линии
    /// </summary>
    public static bool TryGetClosestEnemyFOWVisibleLineSight(Frame f, EntityRef unitRef,
            out EntityRef closestRef,
            bool inUnitWeaponRange) {

      const bool lineSight = true;
      const bool checkFOW  = true;
      const bool enemyOnly = true;

      FP distanceLim = FP.UseableMax;
      if (inUnitWeaponRange) {
        if (f.TryGetPointer<Unit>(unitRef, out var unit)) {
          distanceLim = unit->GetActiveWeaponConfig(f).baseTriggerDistance;
        }
      }

      return TryGetClosestCharacter(f, unitRef,
              distanceLim,
              lineSight,
              checkFOW,
              enemyOnly,
              out closestRef);
    }

    public static bool TryGetClosestCharacter(Frame f, EntityRef sourceRef,
            FP maxDistance,
            bool lineSight,
            bool checkFOW,
            bool enemyOnly,
            out EntityRef closestRef) {

      maxDistance *= maxDistance;

      FP        minDistance = FP.UseableMax;
      FPVector3 position    = FPVector3.Zero;

      var sourceTransform = f.GetPointer<Transform3D>(sourceRef);
      var sourceTeam      = f.GetPointer<Team>(sourceRef);

      closestRef = EntityRef.None;
      var charactersFilter = f.Filter<Transform3D, Unit, Team, Health>();
      while (charactersFilter.NextUnsafe(
              out var e,
              out var transform,
              out var unit,
              out var team,
              out var health)) {

        if (enemyOnly && team->Equals(sourceTeam)) {
          continue;
        }

        if (sourceRef == e) {
          continue;
        }

        if (health->IsDead) {
          continue;
        }

        //  TODO FoW check
        // if (checkFOW && !f.Global->IsCharacterVisible(f, unit, entity))
        //   continue;
        
        if (lineSight && !HasLineSight(f, sourceTransform->Position + FPVector3.Up, transform->Position + FPVector3.Up)) {
          continue;
        }

        var distanceSquared = FPVector3.DistanceSquared(sourceTransform->Position, transform->Position);
        if (maxDistance < distanceSquared) {
          continue;
        }

        if (distanceSquared < minDistance) {
          minDistance = distanceSquared;
          position    = transform->Position;
          closestRef  = e;
        }
      }

      return minDistance != FP.UseableMax;
    }

    public static bool TryGetClosestCharacterDistance(
            Frame f,
            EntityRef sourceRef,
            Transform2D characterTransform,
            FP maxDistance,
            bool checkTeam,
            bool lineSight,
            bool checkFOW,
            out FP distance) {

      bool anyCharacter = TryGetClosestCharacter(f, sourceRef,
              maxDistance,
              lineSight,
              checkFOW,
              checkTeam,
              out var closestCharacter);

      distance = 0;

      if (closestCharacter != EntityRef.None) {
        Transform2D targetTransform = f.Get<Transform2D>(closestCharacter);
        distance = (targetTransform.Position - characterTransform.Position).Magnitude;
      }

      return anyCharacter;
    }

    public static bool TryGetClosestCharacterDirection(Frame f, EntityRef sourceRef,
            Transform3D* characterTransform,
            FP maxDistance,
            bool enemyOnly,
            bool checkFOW,
            bool checkLineSight,
            out FPVector3 enemyDirection) {

      bool anyCharacter = TryGetClosestCharacter(f, sourceRef,
              maxDistance,
              checkLineSight,
              checkFOW,
              enemyOnly,
              out var targetCharacter);

      enemyDirection = FPVector3.Zero;

      if (targetCharacter != EntityRef.None) {
        var targetTransform = f.GetPointer<Transform3D>(targetCharacter);
        enemyDirection = (targetTransform->Position - characterTransform->Position).Normalized;
      }

      return anyCharacter;
    }

    public static bool TryGetClosestEnemyPosition(Frame f, EntityRef source,
            out FPVector3 closestPosition) {
      if (TryGetClosestEnemyCharacter(f, source, out var closest)) {
        closestPosition = f.GetPointer<Transform3D>(closest)->Position;
      }

      closestPosition = default;
      return false;
    }

    public static bool TryGetClosestEnemyCharacter(Frame f, EntityRef sourceRef,
            out EntityRef closest) {

      FP maxDistance = FP.UseableMax;

      return TryGetClosestEnemyCharacter(f, sourceRef, maxDistance, Angle360,
              out closest);
    }

    public static bool TryGetClosestEnemyCharacter(Frame f, EntityRef sourceRef,
            FP maxDistance,
            FP angleLim,
            out EntityRef closestRef) {

      maxDistance *= maxDistance;

      var sourceTeam      = f.GetPointer<Team>(sourceRef);
      var sourceTransform = f.GetPointer<Transform3D>(sourceRef);

      FP        closestDistance = FP.UseableMax;
      FPVector3 closestPosition = FPVector3.Zero;

      closestRef = EntityRef.None;

      var filter = f.Filter<Unit, Transform3D, Team, Health>();
      while (filter.NextUnsafe(
              out var e,
              out var unit,
              out var transform,
              out var team, 
              out var health)) {

        if (sourceTeam->Equals(team)) {
          continue;
        }

        if (sourceRef == e) {
          continue;
        }

        if (health->IsDead) {
          continue;
        }

        var forwardProjection   = sourceTransform->Forward.XZ;
        var directionProjection = sourceTransform->Position.XZ - transform->Position.XZ;
        if (angleLim < FPVector2.RadiansSigned(forwardProjection, directionProjection) * FP.Rad2Deg) {
          continue;
        }

        var distanceSquared = FPVector3.DistanceSquared(sourceTransform->Position, transform->Position);
        if (maxDistance < distanceSquared) {
          continue;
        }

        if (distanceSquared < closestDistance) {
          closestDistance = distanceSquared;
          closestPosition = transform->Position;
          closestRef      = e;
        }
      }

      return closestDistance != FP.UseableMax;

    }
  }
}