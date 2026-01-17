namespace Quantum {
  public unsafe partial struct ItemBox {
    public bool AddItemToBox(Frame f, EntityRef newItemEntity) {
      if (newItemEntity == EntityRef.None) {
        Log.Error("Trying to add Entity.None to itemBox");
        return false;
      }

      var boxEntity = SelfItemBoxEntity;
      var boxItems  = f.ResolveList(this.ItemRefs);

      if (boxItems.Contains(newItemEntity)) {
        Log.Error("Trying to add item to itemBox but it is already exist in items list");
        return false;
      }

      // auto unpack attachments from weapon, except for main menu storage
      if (AutoUnpackNestedItems) {
        if (f.TryGetPointer(newItemEntity, out WeaponItem* newWeaponItem)) {
          foreach (var weaponSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
            var attachmentEntity = newWeaponItem->AttachmentAtSlot(weaponSlot);
            if (newWeaponItem->CanUnassignAttachmentFromSlot(f, weaponSlot, attachmentEntity)) {
              newWeaponItem->UnassignAttachmentFromSlot(f, weaponSlot, attachmentEntity);
              AddItemToBoxImpl(attachmentEntity);
            }
          }
        }
      }

      AddItemToBoxImpl(newItemEntity);

      f.Events.ItemBoxItemsChanged(SelfItemBoxEntity);
      
      return true;

      void AddItemToBoxImpl(EntityRef itemEntity) {
        boxItems.Add(itemEntity);
        var item      = f.GetPointer<Item>(itemEntity);
        var itemAsset = f.FindAsset(item->Asset);
        itemAsset.ChangeItemOwner(f, itemEntity, boxEntity);
      }
    }

    public bool RemoveItemFromItemBox(Frame f, EntityRef itemEntity) {
      if (itemEntity == EntityRef.None) {
        Log.Error("Trying to remove Entity.None from itemBox");
        return false;
      }

      var boxItems = f.ResolveList(this.ItemRefs);
      var removed  = boxItems.Remove(itemEntity);

      if (!removed) {
        Log.Error("Trying to remove item from itemBox but is does not exist in items list");
        return false;
      }

      var item      = f.Get<Item>(itemEntity);
      var itemAsset = f.FindAsset(item.Asset);

      itemAsset.ChangeItemOwner(f, itemEntity, newOwner: EntityRef.None);

      // if (boxItems.Count == 0) {
      //   f.Events.OpenItemBox(SelfItemBoxEntity);
      // }
      f.Events.ItemBoxItemsChanged(SelfItemBoxEntity);

      return true;
    }
  }
}