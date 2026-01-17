namespace Quantum {
  using System;

  [Serializable]
  public unsafe class AmmoBoxItemAsset : WeaponAttachmentItemAsset {
    public override ItemTypes ItemType => ItemTypes.AmmoBox;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Ammo;

    public AmmoTypes AmmoType => this.Def.AmmoType;

    public override bool CanBeAssignedToSlot(Frame f, EntityRef targetEntity, EntityRef itemEntity,
      CharacterLoadoutSlots slot, WeaponAttachmentSlots weaponSlot,
      out ItemAssetAssignFailReason reason) {
      if (!base.CanBeAssignedToSlot(f, targetEntity, itemEntity, slot, weaponSlot, out var baseReason)) {
        reason = baseReason;
        return false;
      }

      var isAmmoTypeValidForWeapon = f.TryGetPointer(targetEntity, out Item* targetItem) &&
                                     f.FindAsset(targetItem->Asset) is WeaponItemAsset targetWeaponItemAsset &&
                                     targetWeaponItemAsset.AmmoType == this.AmmoType;

      if (!isAmmoTypeValidForWeapon) {
        reason = ItemAssetAssignFailReason.SlotNotValidForItem;
        return false;
      }

      reason = ItemAssetAssignFailReason.None;
      return true;
    }
  }
}