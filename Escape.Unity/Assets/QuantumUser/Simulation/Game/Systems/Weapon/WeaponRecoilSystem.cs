namespace Quantum {
  using Photon.Deterministic;

  public unsafe class WeaponRecoilSystem : SystemSignalsOnly, ISignalOnCreateShoot {
    public void OnCreateShoot(Frame f, EntityRef unitRef, EntityRef weaponRef, Weapon* weapon) {
      var unit = f.GetPointer<Unit>(unitRef);

      if (unit->GetActiveWeaponConfig(f) is not { } activeWeaponConfig) {
        return;
      }

      if (!f.TryGetPointer(unitRef, out CharacterSpectatorCamera* spectatorCamera)) {
        return;
      }

      if (f.Has<Bot>(unitRef)) {
        return;
      }

      if (!f.TryGetPointer(unitRef, out InputContainer* input)) {
        return;
      }

      var recoilAnglesMin = activeWeaponConfig.recoilCameraAnglesMin;
      var recoilAnglesMax = activeWeaponConfig.recoilCameraAnglesMax;

      if (unit->Aiming) {
        recoilAnglesMin *= activeWeaponConfig.recoilCoefficientInAimState;
        recoilAnglesMax *= activeWeaponConfig.recoilCoefficientInAimState;
      }

      var recoilAngleX = unit->RNG.Next(recoilAnglesMin.X, recoilAnglesMax.X) * weapon->CurrentStats.recoilXMultiplier;
      var recoilAngleY = unit->RNG.Next(recoilAnglesMin.Y, recoilAnglesMax.Y) * weapon->CurrentStats.recoilYMultiplier;

      spectatorCamera->SpectatorCameraDesiredRotation.Yaw   += recoilAngleX * FP.Deg2Rad;
      spectatorCamera->SpectatorCameraDesiredRotation.Pitch -= recoilAngleY * FP.Deg2Rad;
    }
  }
}