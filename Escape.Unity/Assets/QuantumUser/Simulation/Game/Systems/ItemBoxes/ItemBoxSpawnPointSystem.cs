namespace Quantum.ItemBoxes {
  using System;
  using UnityEngine.Pool;

  public unsafe class ItemBoxSpawnPointSystem : SystemMainThreadFilter<ItemBoxSpawnPointSystem.Filter> {
    public struct Filter {
      public EntityRef          Entity;
      public Transform3D*       Transform3D;
      public ItemBoxSpawnPoint* SpawnPoint;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (!f.IsVerified) {
        return;
      }

      SpawnItemBox(f, ref filter);

      f.Destroy(filter.Entity);
    }

    void SpawnItemBox(Frame f, ref Filter filter) {
      var spawnedItemBox = f.Global->CreateItemBox(f,
        filter.Transform3D->Position,
        filter.SpawnPoint->ItemBoxPrototype,
        keelAliveWithoutItems: true,
        isThrowAwayFeatureLocked: true
      );

      TransformHelper.CopyRotation(f, filter.Entity, spawnedItemBox);

      var itemBox = f.Unsafe.GetPointer<ItemBox>(spawnedItemBox);

      if (filter.SpawnPoint->ItemsInBox.Ptr != default) {
        var itemsInBox = f.ResolveList(filter.SpawnPoint->ItemsInBox);
        for (var itemIndex = 0; itemIndex < itemsInBox.Count; itemIndex++) {
          var itemAssetRef = itemsInBox[itemIndex];
          var itemAsset    = f.FindAsset(itemAssetRef);

          if (itemAsset.excludeFromItemBoxes) {
            continue;
          }

          var itemGuid = DeterministicGuid.Create(
            namespaceId: f.RuntimeConfig.DeterministicGuid,
            name: $"ItemBox:{spawnedItemBox}:Static:{itemIndex}");

          var itemEntity = itemAsset.CreateItemEntity(f, new ItemAssetCreationData(itemGuid.ToString(), itemAssetRef));
          itemBox->AddItemToBox(f, itemEntity);
        }
      }

      if (filter.SpawnPoint->ItemsInBoxBuilder.IsValid) {
        using (ListPool<GameSnapshotLoadoutItem>.Get(out var tempBuilderResult)) {
          var builderAsset = f.FindAsset(filter.SpawnPoint->ItemsInBoxBuilder);

          builderAsset.Build(f, tempBuilderResult, new ItemDropBuildContext {
            RNG = f.RNG,
          });

          for (var itemIndex = 0; itemIndex < tempBuilderResult.Count; itemIndex++) {
            var item      = tempBuilderResult[itemIndex];
            var itemAsset = ItemAssetCreationData.FindAssetByItemKey(f, item.ItemKey);

            if (itemAsset.excludeFromItemBoxes) {
              continue;
            }

            LoadoutGuidsHelper.AssignRandomDeterministicGuids(item,
              DeterministicGuid.Create(f.RuntimeConfig.DeterministicGuid, $"ItemBox:{spawnedItemBox}:Dynamic:{itemIndex}")
            );

            var itemEntity = itemAsset.CreateItemEntity(f, ItemAssetCreationData.FromGameSnapshotLoadoutItem(f, item));
            itemBox->AddItemToBox(f, itemEntity);

            itemBox->TimerToOpen = itemBox->TimeToOpen;
          }
        }
      }
      
      var itemRefs = f.ResolveList(itemBox->ItemRefs);
      if (itemRefs.Count > 0) {
        itemBox->AutoLayoutItemsInTetris(f);
        Log.Info($"ItemBoxSpawnPointSystem: Auto-arranged ItemBox {spawnedItemBox} to {itemBox->Width}x{itemBox->Height} with {itemRefs.Count} items");
      }
    }
  }
}