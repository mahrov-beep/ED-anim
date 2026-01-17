namespace Quantum {
  using System;

  public partial struct CharacterLoadout {
    public bool CanAssignItemToSlot(Frame f, CharacterLoadoutSlots slot, EntityRef newItemEntity,
      AssignOptions options = AssignOptions.None) {
      return AssignItemToSlotInternal(f, slot, newItemEntity, realRun: false, options);
    }
    
    public bool AssignItemToSlot(Frame f, CharacterLoadoutSlots slot, EntityRef newItemEntity) {
      return AssignItemToSlotInternal(f, slot, newItemEntity, realRun: true, AssignOptions.None);
    }

    // чтобы не дублировать одну и ту же логику в методах "Можем_Ли_Надеть" и "Надеть" был добавлен параметр realRun
    // realRun == true означает что нам нужно провести все проверки и ДЕЙСТВИТЕЛЬНО НАЗНАЧИТЬ предмет
    // realRun == false означает что нужно ТОЛЬКО ПРОВЕРИТЬ можем ли мы назначить предмет в слот, но не назначать его
    bool AssignItemToSlotInternal(Frame f, CharacterLoadoutSlots slot, EntityRef newItemEntity, bool realRun, AssignOptions options) {
      if (newItemEntity == EntityRef.None) {
        if (realRun) {
          Log.Error("Trying to assign Entity.None to slot");
        }

        return false;
      }

      if (Array.IndexOf(CharacterLoadoutSlotsExtension.AllValidSlots, slot) == -1) {
        if (realRun) {
          Log.Error("Trying to assign item to Invalid slot");
        }

        return false;
      }

      if ((options & AssignOptions.SKipSlotAlreadyAssignedCheck) == 0 && HasItemAtSlot(slot)) {
        if (realRun) {
          Log.Error("Trying to assign item to non empty slot");
        }

        return false;
      }

      var newItem      = f.Get<Item>(newItemEntity);
      var newItemAsset = f.FindAsset(newItem.Asset);

      if (!newItemAsset.CanBeAssignedToSlot(f, this.SelfUnitEntity, newItemEntity, slot, WeaponAttachmentSlots.Invalid, out var reason)) {
        if (realRun) {
          Log.Error($"Cannot assign item {newItemAsset.ItemKey} to target slot {slot}: {reason}");
        }

        return false;
      }

      if (realRun) {
        this.ItemAtSlot(slot) = newItemEntity;
        newItemAsset.ChangeItemOwner(f, newItemEntity, this.SelfUnitEntity);

        f.Signals.OnCharacterAfterLoadoutSlotAssigned(this.SelfUnitEntity, slot, newItemEntity);

        NotifyLoadoutModified(f, newItemEntity, this.SelfUnitEntity);
      }

      return true;
    }

    public bool CanUnassignItemFromSlot(Frame f, CharacterLoadoutSlots slot, EntityRef oldItemEntity) {
      return UnassignItemFromSlotInternal(f, slot, oldItemEntity, realRun: false, UnassignOptions.None);
    }

    public bool UnassignItemFromSlot(Frame f, CharacterLoadoutSlots slot, EntityRef oldItemEntity) {
      return UnassignItemFromSlotInternal(f, slot, oldItemEntity, realRun: true, UnassignOptions.None);
    }

    bool UnassignItemFromSlotInternal(Frame f, CharacterLoadoutSlots slot, EntityRef oldItemEntity, bool realRun, UnassignOptions options) {
      unsafe {
        if (oldItemEntity == EntityRef.None) {
          if (realRun) {
            Log.Error("Trying to unassign Entity.None item from slot");
          }

          return false;
        }

        if (slot == CharacterLoadoutSlots.Invalid) {
          if (realRun) {
            Log.Error("Trying to unassign item from Invalid slot");
          }

          return false;
        }

        if (this.ItemAtSlot(slot) != oldItemEntity) {
          if (realRun) {
            Log.Error("Trying to unassign mismatched entity from slot");
          }

          return false;
        }
        
        var oldItem      = f.GetPointer<Item>(oldItemEntity);
        var oldItemAsset = f.FindAsset(oldItem->Asset);

        if (realRun) {
          oldItem->IndexI  = 0;
          oldItem->IndexJ  = 0;
          oldItem->Rotated = false;
        }

        if (!oldItemAsset.CanBeUnAssignedFromSlot(f, oldItemEntity, slot, WeaponAttachmentSlots.Invalid, out var reason)) {
          var shouldIgnore = reason == ItemAssetUnassignFailReason.InventoryWeightOverflow &&
                             (options & UnassignOptions.IgnoreInventoryWeightOverflow) != 0;

          if (!shouldIgnore) {
            if (realRun) {
              Log.Error($"Cannot unassign item from target slot: {reason}");
            }

            return false;
          }
        }

        if (realRun) {
          f.Signals.OnCharacterBeforeLoadoutSlotUnassigned(this.SelfUnitEntity, slot, oldItemEntity);

          this.ItemAtSlot(slot) = EntityRef.None;
          oldItemAsset.ChangeItemOwner(f, oldItemEntity, EntityRef.None);

          NotifyLoadoutModified(f, oldItemEntity, this.SelfUnitEntity);
        }

        return true;
      }
    }

    public bool CanAddItemToTrash(Frame f, EntityRef newItemEntity, int indexI, int indexJ, bool rotated, int? preferredInsertIndex = null, byte source = 0) {
      return AddItemToTrashInternal(f, newItemEntity, preferredInsertIndex, realRun: false, indexI: indexI, indexJ: indexJ, rotated: rotated, source);
    }

    public bool AddItemToTrash(Frame f, EntityRef newItemEntity, int indexI, int indexJ, bool rotated, int? preferredInsertIndex = null, byte source = 0) {
      return AddItemToTrashInternal(f, newItemEntity, preferredInsertIndex, realRun: true, indexI, indexJ, rotated, source);
    }

    unsafe bool AddItemToTrashInternal(Frame f, EntityRef newItemEntity, int? preferredInsertIndex, bool realRun, int indexI, int indexJ,
      bool rotated, byte source) {
      if (newItemEntity == EntityRef.None) {
        if (realRun) {
          Log.Error("Trying to add Entity.None to trash");
        }

        return false;
      }

      var newItem      = f.GetPointer<Item>(newItemEntity);
      var newItemAsset = f.FindAsset(newItem->Asset);

      if (realRun) {
        newItem->IndexI  = (byte)indexI;
        newItem->IndexJ  = (byte)indexJ;
        newItem->Rotated = rotated;
      }

      if (source == (byte)TetrisSource.Safe && f.Unsafe.TryGetPointer<CharacterLoadout>(this.SelfUnitEntity, out var loadout)) {
        var safeSlot = loadout->ItemAtSlot(CharacterLoadoutSlots.Safe);

        if (safeSlot != EntityRef.None) {
          var safeItem = f.Get<Item>(safeSlot);
          newItem->SafeGuid = safeItem.MetaGuid;
        }
      }

      if (realRun) {
        var trashItems = this.GetCellsInventory(f, source);

        var hasAlready = trashItems.Contains(newItemEntity);

        if (hasAlready) {
          NotifyLoadoutModified(f, newItemEntity, (TetrisSource)source == TetrisSource.Storage ? this.StorageEntity : this.SelfUnitEntity);

          return true;
        }

        trashItems.Add(newItemEntity);

        if (preferredInsertIndex is { } ind && ind >= 0 && ind < trashItems.Count) {
          for (int i = trashItems.Count - 1; i > ind; i--) {
            trashItems[i] = trashItems[i - 1];
          }

          trashItems[ind] = newItemEntity;
        }

        newItemAsset.ChangeItemOwner(f, newItemEntity, (TetrisSource)source == TetrisSource.Storage ? this.StorageEntity : this.SelfUnitEntity);
        NotifyLoadoutModified(f, newItemEntity, (TetrisSource)source == TetrisSource.Storage ? this.StorageEntity : this.SelfUnitEntity);
      }

      return true;
    }

    public unsafe bool RemoveItemFromTetris(Frame f, EntityRef itemEntity, byte source = (byte)TetrisSource.Inventory) {
      if (itemEntity == EntityRef.None) {
        Log.Error("Trying to remove Entity.None from trash");
        return false;
      }
      
      bool removed;
      
      var trashItems = this.GetCellsInventory(f, source);
      removed = trashItems.Remove(itemEntity);

      if (!removed) {
        Log.Error($"Trying to remove item from trash but is does not exist in trash list (source={source})");
        return false;
      }

      if (source == (byte)TetrisSource.Safe) {
        var itemPtr = f.GetPointer<Item>(itemEntity);
        itemPtr->SafeGuid = null;
      
        f.Signals.OnSafeChanged(this.SelfUnitEntity, SafeChangeKind.Removed, itemEntity);
      }

      var item      = f.Get<Item>(itemEntity);
      var itemAsset = f.FindAsset(item.Asset);

      itemAsset.ChangeItemOwner(f, itemEntity, newOwner: EntityRef.None);

      NotifyLoadoutModified(f, itemEntity, (TetrisSource)source == TetrisSource.Storage ? this.StorageEntity : this.SelfUnitEntity);

      return true;
    }

    public bool AssignOrTrashSwapItemToSlot(Frame f, CharacterLoadoutSlots slot, EntityRef newItemEntity, byte source) {
      return HasItemAtSlot(slot)
        ? TrashSwapItemAtSlotInternal(f, slot, newItemEntity, source)
        : AssignItemToSlot(f, slot, newItemEntity);
    }
    
    public bool CanTrashSwapItemAtSlot(Frame f, CharacterLoadoutSlots slot, EntityRef oldItemRef, EntityRef newItemRef) {
      return HasEnoughFreeTetrisSpaceForItem(f, oldItemRef, out _, RotationType.Find, newItemRef) &&
             CanUnassignItemFromSlot(f, slot, oldItemRef);
    }
    
    public bool CanTrashSwapItemAtSlotFromSafe(Frame f, CharacterLoadoutSlots slot, EntityRef oldItemRef, EntityRef newItemRef) {
      return CanUnassignItemFromSlot(f, slot, oldItemRef);
    }

    bool TrashSwapItemAtSlotInternal(Frame f, CharacterLoadoutSlots slot, EntityRef newItemEntity, byte source) {
      var itemEntity = ItemAtSlot(slot);

      if (itemEntity == EntityRef.None) {
        Log.Error("Trying to trash swap EntityRef.None");
        return false;
      }

      var unassignOptions = UnassignOptions.None;

      if (!HasEnoughFreeTetrisSpaceForItem(f, itemEntity, out var place, RotationType.Find, exceptItem: newItemEntity, source: source)) {
        return false;
      }

      if (!UnassignItemFromSlotInternal(f, slot, itemEntity, realRun: true, unassignOptions)) {
        return false;
      }

      if (!AddItemToTrash(f, itemEntity, place.I, place.J, place.Rotated, source: source)) {
        AssignItemToSlot(f, slot, itemEntity);
        return false;
      }

      return AssignItemToSlot(f, slot, newItemEntity);
    }

    public unsafe void NotifyLoadoutModified(Frame f, EntityRef itemRef, EntityRef owner) {
      this.UpdatedFrame = f.Number;

      if (f.Global->CharacterLoadoutModificationEventEnabled) {
        var itemGuid = f.Get<Item>(itemRef).MetaGuid;

        var marker = f.GetOrAddPointer<CharacterLoadoutModifiedMarker>(this.SelfUnitEntity);

        f.ResolveHashSet(marker->ModifiedItems).Add(itemGuid);
        marker->ModificationFrame = f.Number;
      }
    }

    [Flags]
    public enum AssignOptions {
      None                         = 0,
      SKipSlotAlreadyAssignedCheck = 1 << 0,
    }

    [Flags]
    public enum UnassignOptions {
      None                          = 0,
      IgnoreInventoryWeightOverflow = 1 << 0,
    }
  }
}