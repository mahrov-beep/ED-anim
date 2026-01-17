namespace Quantum.Commands {
  using Photon.Deterministic;

  public unsafe class ThrowAwayItemFromTetrisLoadoutCommand : CharacterCommandBase {
    public EntityRef ItemEntity;
    public byte      Source;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.ItemEntity);
      stream.Serialize(ref this.Source);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("ThrowAwayItemFromSlotLoadoutCommand: loadout not exist");
        return;
      }

      if (!f.TryGet<Transform3D>(characterEntity, out var transform3d)) {
        Log.Error("ThrowAwayItemFromSlotLoadoutCommand: transform3d not exist");
        return;
      }

      if (!f.TryGetPointer<Unit>(characterEntity, out var unit)) {
        Log.Error("ThrowAwayItemFromSlotLoadoutCommand: unit not exist");
        return;
      }

      if (!loadout->RemoveItemFromTetris(f, this.ItemEntity, this.Source)) {
        Log.Error("ThrowAwayItemFromSlotLoadoutCommand: failed to remove item from trash");
        return;
      }

      if (loadout->CanThrowAwayToStorage(f)) {
        if (loadout->HasEnoughFreeTetrisSpaceForItem(f, this.ItemEntity, out var place, source: (byte)TetrisSource.Storage)) {
          loadout->AddItemToTrash(f, this.ItemEntity, place.I, place.J, place.Rotated, source: (byte)TetrisSource.Storage);
          return;
        }
      }
      
      if (unit->NearbyBackpack == EntityRef.None) {
        unit->NearbyBackpack = f.Global->CreateItemBox(f, transform3d.Position, customItemBoxPrototype: f.GameMode.BackpackPrototype);
        TransformHelper.CopyRotation(f, characterEntity, unit->NearbyBackpack);
      }

      var itemBox = f.Unsafe.GetPointer<ItemBox>(unit->NearbyBackpack);

      itemBox->IsBackpack = true;
      
      if (!itemBox->AddItemToBox(f, this.ItemEntity)) {
        Log.Error("ThrowAwayItemFromSlotLoadoutCommand: failed to add item to itemBox");

        var item = f.Get<Item>(this.ItemEntity);
        
        loadout->AddItemToTrash(f, this.ItemEntity, item.IndexI, item.IndexJ, item.Rotated, source: this.Source); // возвращаем обратно если ошибка
        return;
      }
      
      itemBox->AutoLayoutItemsInTetris(f, true);

      if (unit->NearbyBackpack == loadout->StorageEntity) {
        loadout->LoadStorageItems(f, unit->NearbyBackpack);
      }
    }
  }
}