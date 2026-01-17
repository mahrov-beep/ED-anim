namespace Quantum.Commands {
  using System.Collections.Generic;
  using Photon.Deterministic;

  public unsafe class ThrowAwayAllItemsFromTrashLoadoutCommand : CharacterCommandBase {
    public bool IsStorage;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.IsStorage);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("ThrowAwayAllItemsFromTrashLoadoutCommand: loadout not exist");
        return;
      }

      if (!f.TryGet<Transform3D>(characterEntity, out var transform3d)) {
        Log.Error("ThrowAwayAllItemsFromTrashLoadoutCommand: transform3d not exist");
        return;
      }

      if (!f.TryGetPointer<Unit>(characterEntity, out var unit)) {
        Log.Error("ThrowAwayAllItemsFromTrashLoadoutCommand: unit not exist");
        return;
      }

      if (unit->NearbyBackpack == EntityRef.None) {
        unit->NearbyBackpack = f.Global->CreateItemBox(f, transform3d.Position, customItemBoxPrototype: f.GameMode.BackpackPrototype);
        TransformHelper.CopyRotation(f, characterEntity, unit->NearbyBackpack);
      }

      var itemBox = f.Unsafe.GetPointer<ItemBox>(unit->NearbyBackpack);

      itemBox->IsBackpack = true;
      
      var trashItemsCopy = new List<EntityRef>();
      var trashItems     = loadout->GetTrashItems(f);
      
      for (int i = 0; i < trashItems.Count; i++) {
        trashItemsCopy.Add(trashItems[i]);
      }
      
      if (loadout->StorageEntity != EntityRef.None) {
        for (var i = trashItemsCopy.Count - 1; i >= 0; i--) {
          var trashItem = trashItemsCopy[i];
          
          if (!loadout->HasEnoughFreeTetrisSpaceForItem(f, trashItem, out var place, source: (byte)TetrisSource.Storage)) {
            continue;
          }

          loadout->AddItemToTrash(f, trashItem, place.I, place.J, place.Rotated, source: (byte)TetrisSource.Storage);

          if (!loadout->RemoveItemFromTetris(f, trashItem, (byte)TetrisSource.Inventory)) {
            loadout->RemoveItemFromTetris(f, trashItem, source: (byte)TetrisSource.Storage);
            Log.Error("ThrowAwayAllItemsFromTrashLoadoutCommand: failed to remove item from trash");
            continue;
          }
          
          trashItemsCopy.RemoveAt(i);
        }
      }

      if (this.IsStorage) {
        return;
      }
      
      foreach (var trashItem in trashItemsCopy) {
        if (!loadout->RemoveItemFromTetris(f, trashItem, (byte)TetrisSource.Inventory)) {
          Log.Error("ThrowAwayAllItemsFromTrashLoadoutCommand: failed to remove item from trash");
          continue;
        }

        if (!itemBox->AddItemToBox(f, trashItem)) {
          Log.Error("ThrowAwayAllItemsFromTrashLoadoutCommand: failed to add item to itemBox");

          var item = f.Get<Item>(trashItem);
        
          loadout->AddItemToTrash(f, trashItem, item.IndexI, item.IndexJ, item.Rotated, source: (byte)TetrisSource.Inventory); // возвращаем обратно если ошибка
        }
      }

      itemBox->AutoLayoutItemsInTetris(f, true);

      if (unit->NearbyBackpack == loadout->StorageEntity) {
        loadout->LoadStorageItems(f, unit->NearbyBackpack);
      }
    }
  }
}