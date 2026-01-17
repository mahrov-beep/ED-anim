namespace Quantum {
  public partial struct ItemBox {
    public unsafe void CreateFromRuntimeStorage(Frame f, GameSnapshotStorage storage) {
      if (storage == null) {
        Log.Error("ItemBox::CreateFromRuntimeStorage::storage is null");
        return;
      }

      if (storage.items is { } items) {
        foreach (var snapshotItem in items) {
          var itemEntity = CreateItem(snapshotItem);
          if (itemEntity != EntityRef.None) {
            var item = f.GetPointer<Item>(itemEntity);
            item->IndexI = snapshotItem.IndexI;
            item->IndexJ = snapshotItem.IndexJ;
            item->Rotated = snapshotItem.Rotated;
            
            AddItemToBox(f, itemEntity);
          }
        }
      }

      EntityRef CreateItem(GameSnapshotLoadoutItem item) {
        var data = ItemAssetCreationData.FromGameSnapshotLoadoutItem(f, item);
        return data.Asset.IsValid ? f.FindAsset(data.Asset).CreateItemEntity(f, data) : EntityRef.None;
      }
    }
  }
}