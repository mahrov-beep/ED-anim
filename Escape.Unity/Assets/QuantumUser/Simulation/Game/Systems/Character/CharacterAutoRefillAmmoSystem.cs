namespace Quantum {
  public unsafe class CharacterAutoRefillAmmoSystem : SystemMainThreadFilter<CharacterAutoRefillAmmoSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit*             Unit;
      public CharacterLoadout* Loadout;
    }

    public override void Update(Frame f, ref Filter filter) {
      var unit = filter.Unit;

      var activeWeaponRef = unit->ActiveWeaponRef;

      if (!f.TryGetPointers(activeWeaponRef, out WeaponItem* weaponItem, out Weapon* weapon)) {
        return;
      }

      var config = weapon->GetConfig(f);

      if (weapon->BulletsCount >= config.bulletsPerShot) {
        return;
      }

      if (weapon->GetAvailableAmmo(f, activeWeaponRef) > 0) {
        return;
      }

      var ammoItemRef = weaponItem->AttachmentAtSlot(WeaponAttachmentSlots.Ammo);
      if (!f.Exists(ammoItemRef)) {
        return;
      }

      var loadout = filter.Loadout;

      if (!loadout->TryFindMergeableInTrash(f, ammoItemRef, out var mergeSourceItemRef)) {
        return;
      }

      loadout->Merge(f, sourceItemRef: mergeSourceItemRef, targetItemRef: ammoItemRef);
    }
  }
}