namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;
 
  public abstract class PlacementPreviewAbilityItem : SpawnObjectAbilityItem {
    [Header("Preview Settings")]
    public bool alignToCastDirection = true;
    public Mesh previewMesh;
    public Material previewMaterial;
    public FPVector3 previewScaleFP = FPVector3.One;
    public FPVector3 previewRotationOffsetDegFP = FPVector3.Zero;
    [NonSerialized] bool previewRotationOffsetCacheInitialized;
    [NonSerialized] bool previewRotationOffsetIsIdentity = true;
    [NonSerialized] FPQuaternion previewRotationOffsetFp = FPQuaternion.Identity;

    protected virtual void OnEnable() {
      InvalidatePreviewRotationOffset();
    }

#if UNITY_EDITOR
    protected virtual void OnValidate() {
      InvalidatePreviewRotationOffset();
    }
#endif

    public override unsafe Ability.AbilityState UpdateAbility(Frame f, EntityRef ownerRef, Ability* ability) {
      Ability.AbilityState state = ability->Update(f, ownerRef);

      if (state.IsActiveStartTick) {
        onActivateEffects.ApplyAll(f, ownerRef);

        if (TryCalculatePlacementTransform(f, ownerRef, ability, out FPVector3 position, out FPQuaternion rotation)) {
          SpawnPlacedEntity(f, ownerRef, ability, position, rotation);
        }
        else {
          Debug.LogWarning($"[{GetType().Name}] placement transform not available owner={ownerRef}");
        }

        ability->StopAbility(f, ownerRef);
        state.IsActive = false;
        state.IsActiveEndTick = true;
      }

      if (state.IsActiveEndTick) {
        onStopEffects.ApplyAll(f, ownerRef);
      }

      return state;
    }

    protected virtual unsafe void SpawnPlacedEntity(Frame f, EntityRef ownerRef, Ability* ability, FPVector3 position, FPQuaternion rotation) {
      var placedRef = f.Create(spawnPrototype);
      var placedTransform = f.GetPointer<Transform3D>(placedRef);
      placedTransform->Position = position;
      placedTransform->Rotation = rotation;

      if (lifetimeSec > FP._0) {
        f.Set(placedRef, ObjectLifetime.Create(lifetimeSec));
      }

      if (AssignTeam && f.TryGetPointer(ownerRef, out Team* ownerTeam)) {
        f.Set(placedRef, *ownerTeam);
      }

      SetupSpawned(f, placedRef, ownerRef, ability);
    }

    public unsafe virtual bool TryCalculatePlacementTransform(Frame f, EntityRef ownerRef, Ability* ability, out FPVector3 position, out FPQuaternion rotation) {
      CalculatePlacementTransform(f, ownerRef, ability, out position, out rotation);
      return true;
    }

    internal unsafe virtual void CalculatePlacementTransform(Frame f, EntityRef ownerRef, Ability* ability, out FPVector3 position, out FPQuaternion rotation) {
      CalculateSpawnPlacement(f, ownerRef, ability, out FPVector3 spawnPosition, out FPVector3 castDirection);
      FPQuaternion spawnRotation = CalculateSpawnRotation(f, ownerRef, castDirection);

      position = spawnPosition;
      rotation = spawnRotation;
    }

    public FPQuaternion GetPreviewRotation(FPQuaternion baseRotation) {
      EnsurePreviewRotationOffsetCache();
      if (previewRotationOffsetIsIdentity) {
        return baseRotation;
      }

      return baseRotation * previewRotationOffsetFp;
    }

    void EnsurePreviewRotationOffsetCache() {
      if (previewRotationOffsetCacheInitialized) {
        return;
      }

      previewRotationOffsetCacheInitialized = true;

      if (previewRotationOffsetDegFP == FPVector3.Zero) {
        previewRotationOffsetIsIdentity = true;
        previewRotationOffsetFp = FPQuaternion.Identity;
        return;
      }

      previewRotationOffsetFp         = FPQuaternion.Euler(previewRotationOffsetDegFP);
      previewRotationOffsetIsIdentity = false;
    }

    void InvalidatePreviewRotationOffset() {
      previewRotationOffsetCacheInitialized = false;
    }

    protected override unsafe FPQuaternion CalculateSpawnRotation(Frame f, EntityRef ownerRef, FPVector3 castDirection) {
      if (!alignToCastDirection) {
        return f.GetPointer<Transform3D>(ownerRef)->Rotation;
      }

      return base.CalculateSpawnRotation(f, ownerRef, castDirection);
    }
  }
}
