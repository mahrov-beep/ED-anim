namespace Quantum.Commands {
  using Photon.Deterministic;
  using UnityEngine.Pool;
  using UnityEngine.Serialization;

  public unsafe class PickUpBestFromNearbyItemBoxLoadoutCommand : CharacterCommandBase {
    public bool EquipTrash              = true;
    public bool IsBackpack              = false;
    public bool NeedToRemoveFromStorage = true;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.EquipTrash);
      stream.Serialize(ref this.IsBackpack);
      stream.Serialize(ref this.NeedToRemoveFromStorage);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("PickUpBestFromNearbyItemBoxToTrashLoadoutCommand: loadout not exist");
        return;
      }

      if (!f.TryGet<Unit>(characterEntity, out var unit)) {
        Log.Error("PickUpBestFromNearbyItemBoxToTrashLoadoutCommand: unit not exist");
        return;
      }

      if (this.NeedToRemoveFromStorage) {
        EquipFromStorage();
      }
      else {
        var nearbyItemBoxRef = this.IsBackpack ? unit.NearbyBackpack : unit.NearbyItemBox;

        if (nearbyItemBoxRef == EntityRef.None) {
          Log.Error("PickUpBestFromNearbyItemBoxToTrashLoadoutCommand: nearby itemBox is none");
          return;
        }
        
        EquipFromItemBox(nearbyItemBoxRef);
      }

      void EquipFromStorage() {
        var items = loadout->GetStorageItems(f);

        using (ListPool<(EntityRef itemEntity, ItemAsset itemAsset)>.Get(out var itemList)) {
          foreach (var itemEntity in items) {
            var item      = f.GetPointer<Item>(itemEntity);
            var itemAsset = f.FindAsset(item->Asset);
            itemList.Add((itemEntity, itemAsset));
          }

          itemList.Sort((a, b) => {
            var cmp = -a.itemAsset.Def.EquipPriority.CompareTo(b.itemAsset.Def.EquipPriority);

            return cmp != 0 ? cmp : a.itemEntity.Index.CompareTo(b.itemEntity.Index);
          });

          while (itemList.Count > 0) {
            var itemTuple = itemList[0];
            itemList.RemoveAt(0);

            TryPickUpItemFromStorage(itemTuple.itemEntity);
          }
        }
      }

      void EquipFromItemBox(EntityRef nearbyItemBoxRef) {
        var itemBox = f.GetPointer<ItemBox>(nearbyItemBoxRef);

        var items = f.ResolveList(itemBox->ItemRefs);

        using (ListPool<(EntityRef itemEntity, ItemAsset itemAsset)>.Get(out var itemList)) {
          foreach (var itemEntity in items) {
            var item      = f.GetPointer<Item>(itemEntity);
            var itemAsset = f.FindAsset(item->Asset);
            itemList.Add((itemEntity, itemAsset));
          }

          itemList.Sort((a, b) => {
            var cmp = -a.itemAsset.Def.EquipPriority.CompareTo(b.itemAsset.Def.EquipPriority);

            return cmp != 0 ? cmp : a.itemEntity.Index.CompareTo(b.itemEntity.Index);
          });

          while (itemList.Count > 0) {
            var itemTuple = itemList[0];
            itemList.RemoveAt(0);

            TryPickUpItemFromItemBox(itemTuple.itemEntity, itemBox);
          }
        }
      }

      void TryPickUpItemFromStorage(EntityRef itemEntity) {
        var hasSlotForItem = loadout->TryFindSlotForItem(f, itemEntity, out var slot, out var weaponSlot);

        var place = CellsRange.Empty;

        if (!hasSlotForItem && !loadout->HasEnoughFreeTetrisSpaceForItem(f, itemEntity, out place, source: (byte)TetrisSource.Inventory)) {
          return;
        }

        if (!this.EquipTrash && !hasSlotForItem) {
          return;
        }

        if (!loadout->RemoveItemFromTetris(f, itemEntity, source: (byte)TetrisSource.Storage)) {
          return;
        }

        if (!AddToLoadout(f, loadout, itemEntity, slot, weaponSlot, place.I, place.J, place.Rotated, realRun: false)) {
          return;
        }

        var item = f.Get<Item>(itemEntity);

        if (!AddToLoadout(f, loadout, itemEntity, slot, weaponSlot, place.I, place.J, place.Rotated, realRun: true)) {
          loadout->AddItemToTrash(f, itemEntity, item.IndexI, item.IndexJ, item.Rotated, source: (byte)TetrisSource.Storage);
        }
      }

      void TryPickUpItemFromItemBox(EntityRef itemEntity, ItemBox* itemBox) {
        var hasSlotForItem = loadout->TryFindSlotForItem(f, itemEntity, out var slot, out var weaponSlot);

        var place = CellsRange.Empty;

        if (!hasSlotForItem && !loadout->HasEnoughFreeTetrisSpaceForItem(f, itemEntity, out place, source: (byte)TetrisSource.Inventory)) {
          return;
        }

        if (!this.EquipTrash && !hasSlotForItem) {
          return;
        }

        if (!itemBox->RemoveItemFromItemBox(f, itemEntity)) {
          return;
        }

        if (!AddToLoadout(f, loadout, itemEntity, slot, weaponSlot, place.I, place.J, place.Rotated, realRun: false)) {
          return;
        }

        if (!AddToLoadout(f, loadout, itemEntity, slot, weaponSlot, place.I, place.J, place.Rotated, realRun: true)) {
          itemBox->AddItemToBox(f, itemEntity);
        }
      }
    }

    bool AddToLoadout(Frame f, CharacterLoadout* loadout, EntityRef itemEntity,
      CharacterLoadoutSlots slot, WeaponAttachmentSlots weaponSlot, int i, int j, bool rotated, bool realRun) {
      if (slot != CharacterLoadoutSlots.Invalid && weaponSlot != WeaponAttachmentSlots.Invalid) {
        if (!f.TryGetPointer<WeaponItem>(loadout->ItemAtSlot(slot), out var weaponItem)) {
          return false;
        }

        return realRun
          ? weaponItem->AssignAttachmentToSlot(f, weaponSlot, itemEntity)
          : weaponItem->CanAssignAttachmentToSlot(f, weaponSlot, itemEntity);
      }

      if (slot != CharacterLoadoutSlots.Invalid) {
        return realRun
          ? loadout->AssignItemToSlot(f, slot, itemEntity)
          : loadout->CanAssignItemToSlot(f, slot, itemEntity);
      }

      return realRun
        ? loadout->AddItemToTrash(f, itemEntity, i, j, rotated, source: (byte)TetrisSource.Inventory)
        : loadout->CanAddItemToTrash(f, itemEntity, i, j, rotated, source: (byte)TetrisSource.Inventory);
    }
  }
}