namespace Quantum {
  using Commands;
  using Photon.Deterministic;

  public unsafe class ThrowAwayWeaponAttachmentFromSlotLoadoutCommand : CharacterCommandBase {
    public CharacterLoadoutSlots OldSlotType;
    public WeaponAttachmentSlots OldWeaponSlotType;
    public EntityRef             ItemEntity;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref OldSlotType);
      stream.Serialize(ref OldWeaponSlotType);
      stream.Serialize(ref ItemEntity);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("ThrowAwayWeaponAttachmentFromSlotLoadoutCommand: loadout not exist");
        return;
      }

      if (!f.TryGetPointer<Transform3D>(characterEntity, out var transform3d)) {
        Log.Error("ThrowAwayWeaponAttachmentFromSlotLoadoutCommand: transform3d not exist");
        return;
      }

      if (!f.TryGetPointer<Unit>(characterEntity, out var unit)) {
        Log.Error("ThrowAwayWeaponAttachmentFromSlotLoadoutCommand: unit not exist");
        return;
      }

      if (!f.TryGetPointer<WeaponItem>(loadout->ItemAtSlot(this.OldSlotType), out var weaponItem)) {
        Log.Error("ThrowAwayWeaponAttachmentFromSlotLoadoutCommand: item at slot is not WeaponItem");
        return;
      }

      if (!weaponItem->UnassignAttachmentFromSlot(f, this.OldWeaponSlotType, this.ItemEntity)) {
        Log.Error("ThrowAwayWeaponAttachmentFromSlotLoadoutCommand: failed to unassign weapon attachment from old slot");
        return;
      }
      
      if (loadout->CanThrowAwayToStorage(f)) {
        if (loadout->HasEnoughFreeTetrisSpaceForItem(f, this.ItemEntity, out var place, source: (byte)TetrisSource.Storage)) {
          loadout->AddItemToTrash(f, this.ItemEntity, place.I, place.J, place.Rotated, source: (byte)TetrisSource.Storage);
          return;
        }
      }

      if (unit->NearbyBackpack == EntityRef.None) {
        unit->NearbyBackpack = f.Global->CreateItemBox(f, transform3d->Position, f.GameMode.BackpackPrototype);
        TransformHelper.CopyRotation(f, characterEntity, unit->NearbyBackpack);
      }

      var itemBox = f.Unsafe.GetPointer<ItemBox>(unit->NearbyBackpack);
      itemBox->IsBackpack = true;

      if (!itemBox->AddItemToBox(f, this.ItemEntity)) {
        Log.Error("ThrowAwayWeaponAttachmentFromSlotLoadoutCommand: failed to add item to itemBox");

        weaponItem->AssignAttachmentToSlot(f, this.OldWeaponSlotType, this.ItemEntity); // возвращаем обратно если ошибка
        return;
      }
      
      itemBox->AutoLayoutItemsInTetris(f, true);

      if (unit->NearbyBackpack == loadout->StorageEntity) {
        loadout->LoadStorageItems(f, unit->NearbyBackpack);
      }
    }
  }
}