using Photon.Deterministic;

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Core;
  using Physics3D;
  using UnityEngine;

  public static unsafe class PhysicsHelper {
    static int _blockRaycastLayerMask = -1;
    static int _staticLayerMask       = -1;
    static int _unitLayerMask         = -1;

    public static LayerMask GetLagCompensatedUnitLayerMask(Frame f, EntityRef unitEntity) {
      if (f.TryGetPointer(unitEntity, out Unit* unit) && unit->LastLagCompensatedFrame == f.Number) {
        return LagCompensationUtility.GetProxyCollisionLayerMask(unit->PlayerRef);
      }

      return GetUnitLayerMask(f);
    }

    public static LayerMask GetStaticLayerMask(Frame f) {
      if (_staticLayerMask == -1) {
        _staticLayerMask = f.Layers.GetLayerMask("Static", "Static_Far", "Static_Near");
      }

      return _staticLayerMask;
    }

    public static LayerMask GetStaticLayerMask(FrameThreadSafe f) {
      if (_staticLayerMask == -1) {
        _staticLayerMask = f.Layers.GetLayerMask("Static", "Static_Far", "Static_Near");
      }

      return _staticLayerMask;
    }

    public static LayerMask GetBlockRaycastLayerMask(FrameBase f) {
      if (_blockRaycastLayerMask == -1) {
        _blockRaycastLayerMask = f.Layers.GetLayerMask("Static", "Static_Far", "Static_Near", "Unit");
      }

      return _blockRaycastLayerMask;
    }

    public static LayerMask GetBlockRaycastLayerMask(FrameThreadSafe f) {
      if (_blockRaycastLayerMask == -1) {
        _blockRaycastLayerMask = f.Layers.GetLayerMask("Static", "Static_Far", "Static_Near", "Unit");
      }

      return _blockRaycastLayerMask;
    }

    public static LayerMask GetUnitLayerMask(Frame f) {
      if (_unitLayerMask == -1) {
        _unitLayerMask = f.Layers.GetLayerMask("Unit");
      }

      return _unitLayerMask;
    }

    public static Hit3D? Raycast(Frame f, FPVector3 origin, FPVector3 direction, FP distance,
      List<PlayerRef> ignorePlayers, bool computeDetails) {
      var layerMask = GetBlockRaycastLayerMask(f);

      var options = QueryOptions.HitAll;
      if (computeDetails) {
        options |= QueryOptions.ComputeDetailedInfo;
      }

      var hits = f.Physics3D.RaycastAll(origin, direction, distance, layerMask, options);
      hits.SortCastDistance();

      for (var i = 0; i < hits.Count; i++) {
        var hit = hits[i];

        if (f.TryGetPointer(hit.Entity, out Unit* unit) && IsIgnoredPlayer(unit->PlayerRef)) {
          continue;
        }

        return hit;
      }

      return null;

      bool IsIgnoredPlayer(PlayerRef otherPlayer) {
        foreach (var ignorePlayer in ignorePlayers) {
          if (otherPlayer.Equals(ignorePlayer)) {
            return true;
          }
        }

        return false;
      }
    }

    public static void RaycastShapeCollision(Frame f, List<PhysicsTarget> targets, EntityRef selfEntity,
      FPVector3 playerPosition, FPVector3 lookDirection, FP radius, FPVector2 angles, FP targetOffsetY,
      Func<Frame, EntityRef, EntityRef, bool> targetFilter) {
      var layerMask = GetLagCompensatedUnitLayerMask(f, selfEntity);

      // try direct raycast
      {
        var rayHits = f.Physics3D.RaycastAll(playerPosition, lookDirection, radius, layerMask);

        for (int i = 0; i < rayHits.Count; i++) {
          var collider = rayHits[i];

          var lagCompensatedPosition = f.TryGetPointer(collider.Entity, out Transform3D* transform3D)
            ? transform3D->Position
            : FPVector3.Zero;

          if (f.TryGetPointer(collider.Entity, out LagCompensationProxy* lagCompensationProxy)) {
            collider.SetHitEntity(lagCompensationProxy->Target);
          }

          if (collider.Entity == selfEntity) {
            continue;
          }

          if (!targetFilter(f, selfEntity, collider.Entity)) {
            continue;
          }

          var canShoot = CheckTarget(collider, lagCompensatedPosition, out PhysicsTarget physicsTarget, skipAngleCheck: true);

          if (canShoot) {
            targets.Add(physicsTarget);
            return;
          }
        }
      }

      // otherwise try sphere check
      var shape = Shape3D.CreateSphere(radius);

      var colliders = f.Physics3D.OverlapShape(playerPosition, FPQuaternion.Identity, shape, layerMask,
        QueryOptions.HitKinematics);

      for (var i = 0; i < colliders.Count; i++) {
        var collider = colliders[i];

        var lagCompensatedPosition = f.TryGetPointer(collider.Entity, out Transform3D* transform3D)
          ? transform3D->Position
          : FPVector3.Zero;

        if (f.TryGetPointer(collider.Entity, out LagCompensationProxy* lagCompensationProxy)) {
          collider.SetHitEntity(lagCompensationProxy->Target);
        }

        if (collider.Entity == selfEntity) {
          continue;
        }

        if (!targetFilter(f, selfEntity, collider.Entity)) {
          continue;
        }

        var canShoot = CheckTarget(collider, lagCompensatedPosition, out PhysicsTarget physicsTarget);

        if (canShoot) {
          targets.Add(physicsTarget);
        }
      }

      bool CheckTarget(Hit3D hit, FPVector3 targetLagCompensatedPosition, out PhysicsTarget physicsTarget,
        bool skipAngleCheck = false) {
        var colliderHeight = UnitColliderHeightHelper.GetCurrentHeight(f, hit.Entity);
        FP targetAimOffset = GetTargetAimOffset(colliderHeight);
        FPVector3 enemyPosition = targetLagCompensatedPosition + FPVector3.Up * targetAimOffset;
        FPVector3 enemyVector   = enemyPosition - playerPosition;

        physicsTarget = default;

        if (FPVector3.Dot(enemyVector, lookDirection) <= FP._0) {
          return false;
        }

        var verticalPlaneNormal   = FPVector3.Cross(FPVector3.Up, lookDirection);
        var horizontalPlaneNormal = FPVector3.Cross(verticalPlaneNormal, lookDirection);

        var onVertical   = FPVector3.ProjectOnPlane(enemyVector, verticalPlaneNormal);
        var onHorizontal = FPVector3.ProjectOnPlane(enemyVector, horizontalPlaneNormal);

        var verticalAngle   = FPVector3.Angle(lookDirection, onVertical);
        var horizontalAngle = FPVector3.Angle(lookDirection, onHorizontal);

        if (skipAngleCheck || (verticalAngle < angles.Y && horizontalAngle < angles.X)) {
          physicsTarget = new PhysicsTarget();

          physicsTarget.EntityRef                    = hit.Entity;
          physicsTarget.EntityPositionLagCompensated = enemyPosition;
          physicsTarget.Angle                        = FPVector3.Angle(lookDirection, enemyVector);
          physicsTarget.SqrDistance                  = enemyVector.SqrMagnitude;

          return true;
        }

        return false;
      }

      FP GetTargetAimOffset(FP colliderHeight) {
        FP halfHeight = colliderHeight > FP._0 ? colliderHeight * FP._0_50 : FP._0;

        if (targetOffsetY > FP._0) {
          return halfHeight > FP._0 ? FPMath.Min(targetOffsetY, halfHeight) : targetOffsetY;
        }

        return halfHeight;
      }
    }

    public static HitCollection3D OverlapShape(Frame f, Transform3D* transform, int layerMask, Shape3DConfig shape) {
      var queryOptions = QueryOptions.ComputeDetailedInfo | QueryOptions.HitKinematics | QueryOptions.HitStatics;

      var hits = f.Physics3D.OverlapShape(*transform, shape.CreateShape(f), layerMask, queryOptions);

      return hits;
    }

    public struct PhysicsTarget {
      public EntityRef EntityRef;
      public FPVector3 EntityPositionLagCompensated;
      public FP        Angle;
      public FP        SqrDistance;
    }
  }
}
