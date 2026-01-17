namespace Quantum {
  public unsafe class CharacterThrowUsedItemsFromLoadoutSystem : SystemMainThreadFilter<CharacterThrowUsedItemsFromLoadoutSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit*             Unit;
      public CharacterLoadout* Loadout;
    }

    public override void Update(Frame f, ref Filter filter) {
      foreach (var slot in CharacterLoadoutSlotsExtension.AllValidSlots) {
        var slotItemEntity = filter.Loadout->ItemAtSlot(slot);

        if (f.Has<ItemOutOfUses>(slotItemEntity)) {
          filter.Loadout->UnassignItemFromSlot(f, slot, slotItemEntity);
        }
      }

      var trashItems = f.ResolveList(filter.Loadout->TrashItems);
      for (int i = trashItems.Count - 1; i >= 0; i--) {
        var trashItemEntity = trashItems[i];
        if (f.Has<ItemOutOfUses>(trashItemEntity)) {
          filter.Loadout->RemoveItemFromTetris(f, trashItemEntity, (byte)TetrisSource.Inventory);
        }
      }
    }
  }
}