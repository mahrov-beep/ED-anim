namespace Quantum.Commands {
  using Photon.Deterministic;
  using UnityEngine;
  using BitStream = Photon.Deterministic.BitStream;
  public class ReloadWeaponCommand : CharacterCommandBase {
    public override void Serialize(BitStream stream) { }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      var unit = f.GetPointer<Unit>(characterEntity);

      if (unit->IsWeaponChanging) {
        return;
      }

      var weaponRef = unit->ActiveWeaponRef;
      if (weaponRef == EntityRef.None) {
        return;
      }

      if (!f.TryGetPointers(weaponRef, out WeaponItem* weaponItem, out Weapon* weapon)) {
        return;
      }

      if (weapon->IsFullMagazine) {
        return;
      }

      if (weapon->IsReloading) {
        return;
      }

      if (weapon->PreReloadingTimer.IsSet) {
        return;
      }

      var available = weapon->GetAvailableAmmo(f, weaponRef);
      if (available <= 0) {
        if (f.TryGetPointer(characterEntity, out CharacterLoadout* loadout)) {
          var ammoItemRef = weaponItem->AttachmentAtSlot(WeaponAttachmentSlots.Ammo);
          if (ammoItemRef != EntityRef.None) {
            while (available <= 0 && loadout->TryFindMergeableInTrash(f, ammoItemRef, out var mergeSource)) {
              loadout->Merge(f, mergeSource, ammoItemRef);
              available = weapon->GetAvailableAmmo(f, weaponRef);
            }
          }
        }
      }

      if (available <= 0) {
        return;
      }

      weapon->PreReloadingTimer = FrameTimer.FromSeconds(f, weapon->GetConfig(f).preReloadAmmoSeconds);
    }
  }
}