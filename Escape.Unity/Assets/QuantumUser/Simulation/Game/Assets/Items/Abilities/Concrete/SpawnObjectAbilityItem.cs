namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;
  [Serializable]
  public abstract class SpawnObjectAbilityItem : AbilityItemAsset {
    public AssetRef<EntityPrototype> spawnPrototype;

    [Header("Если <= 0, то созданный объект живет вечно. Или уничтожится по истечении времени.")]
    // [RangeEx(-1, 120)]
    public FP lifetimeSec = FP._5;
    // [RangeEx(1, 5)]
    public FP spawnDirectionMultiplayer = FP._1;

    public bool AssignTeam = true;

    protected unsafe virtual void CalculateSpawnPlacement(Frame f, EntityRef ownerRef, Ability* ability, out FPVector3 spawnPosition, out FPVector3 castDirection) {
      var ownerTransform = f.GetPointer<Transform3D>(ownerRef);
      castDirection = GetHorizontalForward(ownerTransform);
      spawnPosition = ownerTransform->Position + (castDirection * spawnDirectionMultiplayer);
    }

    protected unsafe virtual FPQuaternion CalculateSpawnRotation(Frame f, EntityRef ownerRef, FPVector3 castDirection) {
      var ownerTransform = f.GetPointer<Transform3D>(ownerRef);
      FPVector3 direction = castDirection;
      if (direction == FPVector3.Zero) {
        direction = ownerTransform->Forward;
      }

      FPVector3 flattened = new FPVector3(direction.X, FP._0, direction.Z);
      if (flattened == FPVector3.Zero) {
        flattened = GetHorizontalForward(ownerTransform);
      }

      if (flattened == FPVector3.Zero) {
        return ownerTransform->Rotation;
      }

      FPVector3 normalized = flattened.Normalized;
      return FPQuaternion.LookRotation(normalized, FPVector3.Up);
    }

    static unsafe FPVector3 GetHorizontalForward(Transform3D* ownerTransform) {
      FPVector3 forward = ownerTransform->Forward;
      FPVector3 flattened = new FPVector3(forward.X, FP._0, forward.Z);

      if (flattened == FPVector3.Zero) {
        FPVector3 right = ownerTransform->Rotation * FPVector3.Right;
        FPVector3 horizontal = FPVector3.Cross(FPVector3.Up, right);
        if (horizontal == FPVector3.Zero) {
          return FPVector3.Forward;
        }
        return horizontal.Normalized;
      }

      return flattened.Normalized;
    }

    public override unsafe bool TryActivateAbility(Frame f, EntityRef entityRef, Ability* ability) {
      var canActivate = base.TryActivateAbility(f, entityRef, ability);
      if (!canActivate) {
        return false;
      }

      var transform = f.GetPointer<Transform3D>(entityRef);
      var unitPosition  = transform->Position;
      CalculateSpawnPlacement(f, entityRef, ability, out FPVector3 spawnPosition, out _);
      var offsetUp      = FPVector3.Up * FP._0_50;

      // DebugDrawHelper.DrawLine(f,
      //         unitPosition + offsetUp,
      //         spawnPosition + offsetUp,
      //         ColorRGBA.Yellow, FP._10);

      if (!LineOfSightHelper.HasLineSight(f,
              unitPosition + offsetUp,
              spawnPosition + offsetUp)) {
        ability->ResetCooldown();
        ability->StopAbility(f, entityRef);

        return false;
      }

      return true;
    }

    public override unsafe Ability.AbilityState UpdateAbility(Frame f, EntityRef ownerRef, Ability* ability) {
      var state = base.UpdateAbility(f, ownerRef, ability);

      if (state.IsActiveStartTick) {
        var spawnedRef       = f.Create(spawnPrototype);
        var spawnedTransform = f.GetPointer<Transform3D>(spawnedRef);

        CalculateSpawnPlacement(f, ownerRef, ability, out FPVector3 spawnPosition, out FPVector3 castDirection);
        FPQuaternion spawnRotation = CalculateSpawnRotation(f, ownerRef, castDirection);
        spawnedTransform->Position = spawnPosition;
        spawnedTransform->Rotation = spawnRotation;
        
        if (lifetimeSec > FP._0) {
          f.Set(spawnedRef, ObjectLifetime.Create(lifetimeSec));
        }

        if (AssignTeam) {
          f.Set(spawnedRef, *f.GetPointer<Team>(ownerRef));
        }

        SetupSpawned(f, spawnedRef, ownerRef, ability);
      }

      return state;
    }

    protected abstract unsafe void SetupSpawned(Frame f, EntityRef spawnedRef, EntityRef ownerRef, Ability* ability);
  }
}
