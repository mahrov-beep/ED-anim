namespace Quantum {
  using System;
  using System.Collections.Generic;
  using CellsInventory;
  using Collections;
  using Photon.Deterministic;

  public unsafe partial struct CharacterLoadout {
    public ref EntityRef ItemAtSlot(CharacterLoadoutSlots slot) => ref this.SlotItemsRaw[(int)slot];

    public bool HasItemAtSlot(CharacterLoadoutSlots slot) {
      return ItemAtSlot(slot) is var itemEntity && itemEntity != EntityRef.None;
    }

    public bool TryGetItemAtSlot(CharacterLoadoutSlots slot, out EntityRef itemRef) {
      itemRef = ItemAtSlot(slot);
      return itemRef != EntityRef.None;
    }

    public QList<EntityRef> GetCellsInventory(Frame f, byte index) {
      return (TetrisSource)index switch {
        TetrisSource.Inventory => f.ResolveList(this.TrashItems),
        TetrisSource.Safe => f.ResolveList(this.SafeItems),
        TetrisSource.Storage => this.GetStorageItems(f),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
      };
    }

    public QList<EntityRef> GetTrashItems(Frame f) => f.ResolveList(this.TrashItems);
    public QList<EntityRef> GetSafeItems(Frame f) => f.ResolveList(this.SafeItems);
    
    public QList<EntityRef> GetStorageItems(Frame f) {
      var itemBox = f.Get<ItemBox>(this.StorageEntity);
      
      return f.ResolveList(itemBox.ItemRefs);
    } 

    public bool HasItemInLoadout(Frame f, EntityRef itemEntity, byte source) {
      return HasItemInAnySlot(f, itemEntity) || HasItemInTrash(f, itemEntity, source);
    }

    public bool HasItemInAnySlot(Frame f, EntityRef itemEntity) {
      foreach (var slot in CharacterLoadoutSlotsExtension.AllValidSlots) {
        if (ItemAtSlot(slot) == itemEntity) {
          return true;
        }

        if (f.TryGet(ItemAtSlot(slot), out WeaponItem weaponItem)) {
          foreach (var weaponSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
            if (weaponItem.AttachmentAtSlot(weaponSlot) == itemEntity) {
              return true;
            }
          }
        }
      }

      return false;
    }

    public bool HasItemInTrash(Frame f, EntityRef itemEntity, byte source) {
      foreach (var trashItem in GetCellsInventory(f, index: source)) {
        if (trashItem == itemEntity) {
          return true;
        }
      }

      return false;
    }

    public FP GetTotalItemsWeightLimit(Frame f) {
      if (f.TryGetPointer(SelfUnitEntity, out Unit* unit)) {
        return unit->CurrentStats.maxWeight;
      }

      return FP._0;
    }

    public (int width, int height) GetLoadoutParameters(Frame f, byte source = 0) {
      if (source == (byte)TetrisSource.Safe) {
        if (f.TryGetPointer(SelfUnitEntity, out CharacterSafe* safe)) {
          return safe->GetSafeParameters(f);
        }
      }
      
      if (source == (byte)TetrisSource.Storage) {
        if (f.TryGetPointer(SelfUnitEntity, out Unit* unitStorage)) {
          if (this.StorageEntity != EntityRef.None) {
            return GetStorageParameters(f, this.StorageEntity);
          }
        }
        return (0, 0);
      }
      
      if (f.TryGetPointer(SelfUnitEntity, out Unit* unit)) {
        return (unit->CurrentStats.loadoutWidth.AsFP.AsInt, unit->CurrentStats.loadoutHeight.AsFP.AsInt);
      }

      return (0, 0);
    }

    public int GetLoadoutQuality(Frame f) {
      var quality = 0;

      foreach (var slot in CharacterLoadoutSlotsExtension.UsedForQualitySlots) {
        var itemAtSlot = ItemAtSlot(slot);
        if (f.TryGetPointer(itemAtSlot, out Item* item)) {
          quality += f.FindAsset(item->Asset).Def.Quality;
        }
      }

      return quality;
    }

    public bool HasEnoughFreeSpaceForItem(Frame f, EntityRef itemEntity) {
      if (HasItemInTrash(f, itemEntity, (byte)TetrisSource.Inventory)) {
        return true; // already in trash, we can move item without check
      }

      return true;
    }

    public FP GetTotalItemsWeight(Frame f) {
      var weight = FP._0;

      foreach (var trashItem in GetTrashItems(f)) {
        weight += Item.GetItemWeight(f, trashItem);
      }

      return weight;
    }

    void FillLoadout(Frame f, ref CellsInventoryAccess grid, byte source, EntityRef exceptItem = default) {
      var items = this.GetCellsInventory(f, source);
      
      var ranges = new List<CellsRange>();
      
      foreach (var trashItem in items) {
        if (trashItem == exceptItem) {
          continue;
        }
        
        ranges.Add(Item.GetItemTetris(f, trashItem, withRotation: true));
      }
      
      CellsInventoryMath.FillLoadout(ref grid, ranges.ToArray());
    }

    public bool TryGetItemAt(Frame f, int targetI, int targetJ, out EntityRef itemRef, byte source = 0) {
      var items = this.GetCellsInventory(f, source);
      
      foreach (var trashItem in items) {
        var tetrisMetrics = Item.GetItemTetris(f, trashItem, withRotation: true);

        if (tetrisMetrics.Contains(targetI, targetJ)) {
          itemRef = trashItem;
          return true;
        }
      }

      itemRef = EntityRef.None;
      return false;
    }

    public bool HasEnoughFreeTetrisSpaceForItem(Frame f, EntityRef itemEntity, out CellsRange place,
      RotationType rotationType = RotationType.Find, EntityRef exceptItem = default, byte source = 0) {
      if (HasItemInTrash(f, itemEntity, source)) {
        place = Item.GetItemTetris(f, itemEntity, withRotation: true);
        return true; // already in trash, we can move item without check
      }

      var itemMetrics = Item.GetItemTetris(f, itemEntity, withRotation: false);

      var loadout = CellsInventoryAccess.Rent(this.GetLoadoutParameters(f, source));
      try {
        this.FillLoadout(f, ref loadout, exceptItem: exceptItem == default ? itemEntity : exceptItem, source: source);

        return CellsInventoryMath.TryFindFreeSpaceForItem(ref loadout, (width: itemMetrics.Width, height: itemMetrics.Height), out place,
          RotationTypeToSearchMode(rotationType));
      }
      finally {
        loadout.Dispose();
      }
    }

    public bool CanBePlaceIn(Frame f, EntityRef itemEntity, int indexI, int indexJ, out CellsRange dropRange, RotationType rotationType, byte source) {
      var grid = CellsInventoryAccess.Rent(this.GetLoadoutParameters(f, source));
      try {
        this.FillLoadout(f, ref grid, source, exceptItem: itemEntity);
        var itemMetrics = Item.GetItemTetris(f, itemEntity, withRotation: false);
        return CellsInventoryMath.CanBePlaceIn(ref grid, itemMetrics.WithIJ(indexI, indexJ), out dropRange,
          RotationTypeToSearchMode(rotationType));
      }
      finally {
        grid.Dispose();
      }
    }

    static CellsInventoryRotationSearchMode RotationTypeToSearchMode(RotationType rotationType) {
      return rotationType switch {
        RotationType.Find => CellsInventoryRotationSearchMode.Find,
        RotationType.Default => CellsInventoryRotationSearchMode.Default,
        RotationType.Rotated => CellsInventoryRotationSearchMode.Rotated,
        _ => throw new ArgumentOutOfRangeException(nameof(rotationType), rotationType, null),
      };
    }
    
    public void LoadStorageItems(Frame f, EntityRef itemBoxEntity) {
      if (!f.TryGetPointer<ItemBox>(itemBoxEntity, out var itemBox)) {
        Log.Error("LoadStorageItems: ItemBox not found");
        return;
      }

      this.StorageEntity = itemBoxEntity;
    }

    public (int width, int height) GetStorageParameters(Frame f, EntityRef itemBoxEntity) {
      if (f.TryGetPointer<ItemBox>(itemBoxEntity, out var itemBox)) {
        return (itemBox->Width, itemBox->Height);
      }
      
      return (0, 0);
    }

    public bool CanThrowAwayToStorage(Frame f) {
      if (StorageEntity == EntityRef.None) {
        return false;
      }

      if (f.TryGetPointer(StorageEntity, out ItemBox* storageItemBox) && storageItemBox->IsThrowAwayFeatureLocked) {
        return false;
      }

      return true;
    }
  }

  public enum RotationType {
    Find,
    Default,
    Rotated,
  }
}