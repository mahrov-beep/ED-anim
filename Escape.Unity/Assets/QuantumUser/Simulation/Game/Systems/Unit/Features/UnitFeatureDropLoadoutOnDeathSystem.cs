namespace Quantum {
  public unsafe class UnitFeatureDropLoadoutOnDeathSystem : SystemMainThreadFilter<UnitFeatureDropLoadoutOnDeathSystem.Filter>,
    ISignalOnUnitDead,
    ISignalOnOpenItemBox {
    public struct Filter {
      public EntityRef Entity;

      public Unit*                          Unit;
      public CharacterLoadout*              Loadout;
      public UnitFeatureDropLoadoutOnDeath* Feature;
    }

    public override void Update(Frame f, ref Filter filter) {
    }

    public void OnUnitDead(Frame f, EntityRef e) {
      if (f.TryGet(e, out Transform3D transform3D) &&
          f.Has<CharacterLoadout>(e) &&
          f.Has<UnitFeatureDropLoadoutOnDeath>(e)) {
        // Создаем пустой ящик при смерти юнита.
        // Предметы будут переложены в ящик только в момент открытия ящика.
        // Это нужно чтобы экипировка не попадала с юнита сразу при его смерти
        var itemBoxEntity = f.Global->CreateItemBox(f, transform3D.Position, keelAliveWithoutItems: true, isThrowAwayFeatureLocked: true);
        f.Add<DropFromUnitMarker>(itemBoxEntity, out var dropFromUnit);
        dropFromUnit->SourceUnitRef = e;
      }
    }

    public void OnOpenItemBox(Frame f, EntityRef itemBoxRef) {
      if (f.TryGetPointer(itemBoxRef, out ItemBox* itemBox) &&
          f.TryGetPointer(itemBoxRef, out DropFromUnitMarker* dropFromUnit) &&
          f.TryGetPointer(dropFromUnit->SourceUnitRef, out CharacterLoadout* loadout)) {
        // При попытке открыть ящик который был создан из юнита,
        // перекладываем предметы из лодаута в этот ящик

        foreach (var slot in CharacterLoadoutSlotsExtension.AllValidSlotsExceptBackpack) {
          if (slot == CharacterLoadoutSlots.Safe) {
            continue;
          }
          this.UnassignItemFromSlot(f, itemBox, loadout, slot);
        }

        var trashItems = f.ResolveList(loadout->TrashItems);

        for (int i = trashItems.Count - 1; i >= 0; i--) {
          var trashItemEntity = trashItems[i];

          var trashItem      = f.GetPointer<Item>(trashItemEntity);
          var trashItemAsset = f.FindAsset(trashItem->Asset);

          if (!string.IsNullOrEmpty(trashItem->SafeGuid)) {
            continue;
          }

          if (trashItemAsset.dropOnDeath == false) {
            continue;
          }

          if (loadout->RemoveItemFromTetris(f, trashItemEntity, (byte)TetrisSource.Inventory)) {
            itemBox->AddItemToBox(f, trashItemEntity);
          }
        }

        this.UnassignItemFromSlot(f, itemBox, loadout, CharacterLoadoutSlots.Backpack);
        
        var itemRefs = f.ResolveList(itemBox->ItemRefs);
        if (itemRefs.Count > 0) {
          itemBox->AutoLayoutItemsInTetris(f);
          Log.Info($"UnitFeatureDropLoadoutOnDeathSystem: Auto-arranged death ItemBox to {itemBox->Width}x{itemBox->Height} with {itemRefs.Count} items");
        }

        if (f.Exists(itemBox->OpenerUnitRef)) {
          if (f.Unsafe.TryGetPointer<CharacterLoadout>(itemBox->OpenerUnitRef, out var openerLoadout)) {
            openerLoadout->LoadStorageItems(f, itemBox->SelfItemBoxEntity);
            Log.Info($"OpenItemBoxCommand: Loaded {openerLoadout->GetStorageItems(f).Count} items from ItemBox to Storage");
          } 
        }
      }
    }

    private void UnassignItemFromSlot(Frame f, ItemBox* itemBox, CharacterLoadout* loadout, CharacterLoadoutSlots slot) {
      var slotItemEntity = loadout->ItemAtSlot(slot);
      if (slotItemEntity != EntityRef.None) {
        if (slot == CharacterLoadoutSlots.Safe) {
          return;
        }
        var slotItem      = f.GetPointer<Item>(slotItemEntity);
        var slotItemAsset = f.FindAsset(slotItem->Asset);

        if (slotItemAsset.dropOnDeath == false) {
          return;
        }

        if (loadout->UnassignItemFromSlot(f, slot, slotItemEntity)) {
          itemBox->AddItemToBox(f, slotItemEntity);
        }
      }
    }
  }
}