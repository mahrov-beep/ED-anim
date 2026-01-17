namespace Quantum {
  using System;
  using Photon.Deterministic;

  [Serializable]
  public class TurretSpawnAbility : PlacementPreviewAbilityItem {

    protected override unsafe void SetupSpawned(Frame f,
            EntityRef spawnedRef, EntityRef ownerRef, Ability* ability) {

      var turret = f.GetPointer<Turret>(spawnedRef);
      turret->Owner = ownerRef;

      if (alignToCastDirection) {
        TryApplyCastRotation(f, spawnedRef, ownerRef);
      }

      if (f.TryGetPointer(ownerRef, out Team* ownerTeam)) {
        f.Set(spawnedRef, *ownerTeam);
      }

      var weaponConfig = f.FindAsset(turret->WeaponConfig);
      var weaponRef = weaponConfig.CreateItemEntity(f,
              new (string.Empty, weaponConfig));

      weaponConfig.ChangeItemOwner(f, weaponRef, spawnedRef);

      var unit = f.GetPointer<Unit>(spawnedRef);
      unit->TryChangeWeapon(f, weaponRef);
    }

    static unsafe void TryApplyCastRotation(Frame f, EntityRef spawnedRef, EntityRef ownerRef) {
      if (!f.TryGetPointer(ownerRef, out Unit* ownerUnit)
              || !f.TryGetPointer(spawnedRef, out Transform3D* spawnedTransform)) {
        return;
      }

      FPQuaternion finalRotation = ownerUnit->ActiveAbilityInfo.CastRotation;
      spawnedTransform->Rotation = finalRotation;

      if (!f.TryGetPointer(spawnedRef, out CharacterSpectatorCamera* spectatorCamera)) {
        return;
      }

      FPYawPitchRoll yawPitchRoll = FPQuaternionHelper.AsYawPitchRoll(finalRotation);
      spectatorCamera->CharacterCurrentRotation = yawPitchRoll.Yaw;
      spectatorCamera->CharacterCurrentPitchRotation = yawPitchRoll.Pitch;
      spectatorCamera->SpectatorCameraCurrentRotation = yawPitchRoll;
      spectatorCamera->SpectatorCameraDesiredRotation = yawPitchRoll;
    }
  }
}
