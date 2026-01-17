namespace Quantum {
  public partial struct CharacterLoadout {
    public unsafe void CreateItemsFromRuntimeLoadout(Frame f, GameSnapshotLoadout loadout) {
      if (loadout == null) {
        Log.Error("PlayerLoadout::CreateItemEntities::loadout is null");
        return;
      }

      CreationFrame = f.Number;

      if (loadout.SlotItems is { } itemSlots) {
        // рюкзак должен быть первым предметом так как в нем задается вместимость
        foreach (var slotType in CharacterLoadoutSlotsExtension.AllValidSlotsBackpackFirst) {
          var slotIndex = (int)slotType;
          if (slotIndex < itemSlots.Length && itemSlots[slotIndex] != null) {
            var item = CreateItem(itemSlots[slotIndex]);
            if (item != EntityRef.None) {
              this.AssignItemToSlot(f, slotType, item);
            }
          }
        }
      }

      if (loadout.TrashItems is { } trashItems) {
        foreach (var trashItem in trashItems) {
          var entity = CreateItem(trashItem);
          if (entity == EntityRef.None) {
            continue;
          }

          var source = string.IsNullOrEmpty(trashItem.SafeGuid) ? (byte)TetrisSource.Inventory : (byte)TetrisSource.Safe;
          this.AddItemToTrash(f, entity, trashItem.IndexI, trashItem.IndexJ, trashItem.Rotated, source: source);
        }
      }

      // сразу заряжаем оружие чтобы была не нужна переарядка
      Weapon.LoadAmmoFromAmmoBoxToWeapon(f, this.ItemAtSlot(CharacterLoadoutSlots.PrimaryWeapon));
      Weapon.LoadAmmoFromAmmoBoxToWeapon(f, this.ItemAtSlot(CharacterLoadoutSlots.SecondaryWeapon));

      return;

      EntityRef CreateItem(GameSnapshotLoadoutItem item) {
        var data = ItemAssetCreationData.FromGameSnapshotLoadoutItem(f, item);
        return data.Asset.IsValid ? f.FindAsset(data.Asset).CreateItemEntity(f, data) : EntityRef.None;
      }
    }
  }
}