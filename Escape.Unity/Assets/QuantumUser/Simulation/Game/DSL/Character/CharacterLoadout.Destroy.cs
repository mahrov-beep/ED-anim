namespace Quantum {
  public unsafe partial struct CharacterLoadout {
    public void DestroyAllItems(Frame f) {
      for (var i = CharacterLoadoutSlotsExtension.AllValidSlotsBackpackFirst.Length - 1; i >= 0; i--) {
        var slot = CharacterLoadoutSlotsExtension.AllValidSlotsBackpackFirst[i];
        if (slot == CharacterLoadoutSlots.Safe) {
          continue;
        }
        DestroyItem(ItemAtSlot(slot));
      }

      var trashItems = f.ResolveList(TrashItems);
      foreach (var trashItem in trashItems) {
        var it = f.Get<Item>(trashItem);
        if (!string.IsNullOrEmpty(it.SafeGuid)) {
          continue;
        }
        DestroyItem(trashItem);
      }

      void DestroyItem(EntityRef itemEntity) {
        if (itemEntity == EntityRef.None) {
          return;
        }

        var item      = f.GetPointer<Item>(itemEntity);
        var itemAsset = f.FindAsset(item->Asset);
        itemAsset.DestroyItemEntity(f, itemEntity);
      }
    }
  }
}