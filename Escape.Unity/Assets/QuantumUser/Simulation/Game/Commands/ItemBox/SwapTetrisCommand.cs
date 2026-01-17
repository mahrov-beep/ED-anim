namespace Quantum.Commands {
  using Photon.Deterministic;

  public unsafe class SwapTetrisCommand : CharacterCommandBase {
    public EntityRef             ItemEntity;
    public int                   IndexI;
    public int                   IndexJ;
    public bool                  Rotated;
    public CharacterLoadoutSlots Slot;
    public WeaponAttachmentSlots WeaponSlot;
    public bool                  SmartAssignToSlot;
    public byte                  DestinationSource;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.ItemEntity);
      stream.Serialize(ref this.IndexI);
      stream.Serialize(ref this.IndexJ);
      stream.Serialize(ref this.Rotated);
      stream.Serialize(ref this.Slot);
      stream.Serialize(ref this.WeaponSlot);
      stream.Serialize(ref this.SmartAssignToSlot);
      stream.Serialize(ref this.DestinationSource);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("PickUpItemFromNearbyItemBoxToTrashLoadoutCommand: loadout not exist");
        return;
      }

      if (!f.TryGet<Unit>(characterEntity, out var unit)) {
        Log.Error("PickUpItemFromNearbyItemBoxToTrashLoadoutCommand: unit not exist");
        return;
      }

      if (unit.NearbyItemBox == EntityRef.None && unit.NearbyBackpack == EntityRef.None) {
        Log.Error("PickUpItemFromNearbyItemBoxToTrashLoadoutCommand: nearby itemBox is none");
        return;
      }

      var fromSource = this.DestinationSource == (byte)TetrisSource.Storage ? (byte)TetrisSource.Inventory : (byte)TetrisSource.Storage;

      if (!loadout->RemoveItemFromTetris(f, this.ItemEntity, fromSource)) {
        Log.Error("PickUpItemFromNearbyItemBoxToTrashLoadoutCommand: failed to remove item from itemBox");
        return;
      }

      // пытаемся сразу положить предмет в слот если SmartAssignToSlot == true
      if (SmartAssignToSlot && Slot == CharacterLoadoutSlots.Invalid && WeaponSlot == WeaponAttachmentSlots.Invalid) {
        loadout->TryFindSlotForItem(f, this.ItemEntity, out this.Slot, out this.WeaponSlot);
      }
      
      var item = f.Get<Item>(this.ItemEntity);

      var dropRange = CellsRange.FromIJWH(0, 0, 0, 0, this.Rotated);
      if (this.Slot == CharacterLoadoutSlots.Invalid && !loadout->HasEnoughFreeTetrisSpaceForItem(f, this.ItemEntity, out dropRange, 
        RotationType.Find, source: this.DestinationSource)) {
        loadout->AddItemToTrash(f, this.ItemEntity, item.IndexI, item.IndexJ, item.Rotated, source: fromSource);

        return;
      }
      
      if (!AddToLoadout(f, loadout, item, dropRange.I, dropRange.J, dropRange.Rotated)) {
        Log.Error("PickUpItemFromNearbyItemBoxToTrashLoadoutCommand: failed to add item to loadout");

        loadout->AddItemToTrash(f, this.ItemEntity, item.IndexI, item.IndexJ, item.Rotated, source: fromSource);
        return;
      }
    }

    bool AddToLoadout(Frame f, CharacterLoadout* loadout, Item item, int indexI, int indexJ, bool rotated) {
      var useTrashSwap = f.GameMode.rule != GameRules.MainMenuStorage;

      if (this.Slot != CharacterLoadoutSlots.Invalid && this.WeaponSlot != WeaponAttachmentSlots.Invalid) {
        if (!f.TryGetPointer<WeaponItem>(loadout->ItemAtSlot(this.Slot), out var weaponItem)) {
          Log.Error("PickUpItemFromNearbyItemBoxToTrashLoadoutCommand: trying to add to weaponSlot but item at slot is not weapon");
          return false;
        }

        if (weaponItem->HasAttachmentAtSlot(this.WeaponSlot)) {
          var attachmentAtSlot = weaponItem->AttachmentAtSlot(this.WeaponSlot);

          if (useTrashSwap) {
            weaponItem->MoveAttachmentToLoadoutTrash(f, this.WeaponSlot, loadout, indexI, indexJ, rotated, this.DestinationSource);
          }
          else if (weaponItem->CanUnassignAttachmentFromSlot(f, this.WeaponSlot, attachmentAtSlot)) {
            weaponItem->UnassignAttachmentFromSlot(f, this.WeaponSlot, attachmentAtSlot);
            loadout->AddItemToTrash(f, attachmentAtSlot, item.IndexI, item.IndexJ, item.Rotated, source: (byte)TetrisSource.Storage);
          }
        }

        return weaponItem->AssignAttachmentToSlot(f, this.WeaponSlot, this.ItemEntity);
      }

      if (Slot != CharacterLoadoutSlots.Invalid) {
        if (useTrashSwap) {
          return loadout->AssignOrTrashSwapItemToSlot(f, this.Slot, this.ItemEntity, (byte)TetrisSource.Storage);
        }

        if (loadout->HasItemAtSlot(this.Slot)) {
          var itemAtSlot = loadout->ItemAtSlot(this.Slot);
          if (loadout->CanUnassignItemFromSlot(f, this.Slot, itemAtSlot)) {
            loadout->UnassignItemFromSlot(f, this.Slot, itemAtSlot);
            loadout->AddItemToTrash(f, itemAtSlot, item.IndexI, item.IndexJ, item.Rotated, source: (byte)TetrisSource.Storage);
          }
        }

        return loadout->AssignItemToSlot(f, this.Slot, this.ItemEntity);
      }

      return loadout->AddItemToTrash(f, this.ItemEntity, indexI, indexJ, rotated, source: this.DestinationSource);
    }
  }

}