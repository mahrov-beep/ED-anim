namespace Quantum {
  using Commands;
  using Photon.Deterministic;

  public unsafe class MoveWeaponAttachmentFromTetrisToSlotLoadoutCommand : CharacterCommandBase {
    public CharacterLoadoutSlots NewSlotType;
    public WeaponAttachmentSlots NewWeaponSlotType;
    public EntityRef             ItemEntity;
    public byte                  Source;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref NewSlotType);
      stream.Serialize(ref NewWeaponSlotType);
      stream.Serialize(ref ItemEntity);
      stream.Serialize(ref Source);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("MoveWeaponAttachmentFromTrashToSlotLoadoutCommand: loadout not exist");
        return;
      }

      if (!f.TryGetPointer<WeaponItem>(loadout->ItemAtSlot(this.NewSlotType), out var weaponItem)) {
        Log.Error("MoveWeaponAttachmentFromTrashToSlotLoadoutCommand: item at new slot is not WeaponItem");
        return;
      }

      if (!loadout->RemoveItemFromTetris(f, this.ItemEntity, this.Source)) {
        Log.Error("MoveWeaponAttachmentFromTrashToSlotLoadoutCommand: failed to remove item from trash");
        return;
      }

      var newItem = f.Get<Item>(this.ItemEntity);

      if (weaponItem->HasAttachmentAtSlot(this.NewWeaponSlotType)) {
        var item = f.Get<Item>(this.ItemEntity);

        weaponItem->MoveAttachmentToLoadoutTrash(f, this.NewWeaponSlotType, loadout, newItem.IndexI, newItem.IndexJ, item.Rotated, this.Source);
      }

      if (!weaponItem->AssignAttachmentToSlot(f, this.NewWeaponSlotType, this.ItemEntity)) {
        Log.Error("MoveWeaponAttachmentFromTrashToSlotLoadoutCommand: failed to assign weapon attachment to weapon slot");

        var item = f.Get<Item>(this.ItemEntity);
        
        loadout->AddItemToTrash(f, this.ItemEntity, item.IndexI, item.IndexJ, item.Rotated); // возвращаем обратно если ошибка
        return;
      }
    }
  }
}