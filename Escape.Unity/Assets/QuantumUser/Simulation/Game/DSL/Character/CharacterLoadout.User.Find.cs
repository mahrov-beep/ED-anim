namespace Quantum {
  using System;

  public unsafe partial struct CharacterLoadout {
    public bool TryFindSlotForItem(Frame f, EntityRef itemEntity,
      out CharacterLoadoutSlots slot, out WeaponAttachmentSlots weaponSlot,
      AssignOptions options = AssignOptions.None) {
      slot       = CharacterLoadoutSlots.Invalid;
      weaponSlot = WeaponAttachmentSlots.Invalid;

      if (!f.TryGetPointer(itemEntity, out Item* item)) {
        return false;
      }

      var itemAsset = f.FindAsset(item->Asset);

      foreach (var possibleSlot in itemAsset.validSlots) {
        if (CanAssignItemToSlot(f, possibleSlot, itemEntity, options)) {
          slot = possibleSlot;
          return true;
        }
      }

      if (itemAsset is WeaponAttachmentItemAsset) {
        foreach (var possibleSlot in CharacterLoadoutSlotsExtension.WeaponSlots) {
          if (ItemAtSlot(possibleSlot) is var atSlotEntity &&
              atSlotEntity != EntityRef.None &&
              f.TryGetPointer(atSlotEntity, out Item* atSlotItem) &&
              f.TryGetPointer(atSlotEntity, out WeaponItem* atSlotWeaponItem) &&
              f.FindAsset(atSlotItem->Asset) is WeaponItemAsset atSlotWeaponItemAsset) {
            if (atSlotWeaponItemAsset.attachmentsSchema == null) {
              continue;
            }

            foreach (var possibleWeaponSlot in atSlotWeaponItemAsset.attachmentsSchema.slots) {
              if (atSlotWeaponItem->CanAssignAttachmentToSlot(f, possibleWeaponSlot, itemEntity, options)) {
                slot       = possibleSlot;
                weaponSlot = possibleWeaponSlot;
                return true;
              }
            }
          }
        }
      }

      return false;
    }

    // Считает суммарное количество оставшихся использований у предметов, которые можно влить в targetItemRef.
    // Получить конкретный предмет можно через TryFindMergeableInTrash
    public int GetAllMergeableInTrashRemainingUsagesSum(Frame f, EntityRef targetItemRef) {
      var items = GetTrashItems(f);
      var count = 0;

      for (var i = 0; i < items.Count; i++) {
        var itemRef = items[i];

        if (CanMerge(f, itemRef, targetItemRef)) {
          count += Item.GetRemainingUsages(f, itemRef);
        }
      }

      return count;
    }

    // Ищет предмет в trash инвентаре который можно влить В targetItemRef.
    public bool TryFindMergeableInTrash(Frame f, EntityRef targetItemRef, out EntityRef sourceItemRef, bool skipEmptyItems = true) {
      var items = GetTrashItems(f);

      for (var i = 0; i < items.Count; i++) {
        var itemRef = items[i];

        if (skipEmptyItems && Item.GetRemainingUsages(f, itemRef) == 0) {
          continue;
        }

        if (CanMerge(f, itemRef, targetItemRef)) {
          sourceItemRef = itemRef;
          return true;
        }
      }

      sourceItemRef = EntityRef.None;
      return false;
    }

    public bool TryFindGrenadeInTrash(Frame f, out EntityRef grenadeItem, bool andDequeue = false) {
      return TryFindItemInTrash(f, static (_, asset) => asset is GrenadeItemAsset, out grenadeItem, andDequeue, 0);
    }

    public bool TryFindHealBoxInTrash(Frame f, out EntityRef grenadeItem, bool andDequeue = false) {
      return TryFindItemInTrash(f, static (_, asset) => asset is HealBoxItemAsset, out grenadeItem, andDequeue, 0);
    }

    public bool TryFindAmmoBoxInTrash(Frame f, out EntityRef grenadeItem, bool andDequeue = false) {
      return TryFindItemInTrash(f, static (_, asset) => asset is AmmoBoxItemAsset, out grenadeItem, andDequeue, 0);
    }

    public bool TryFindAmmoBoxInTrash(Frame f, WeaponItemAsset targetWeapon, out EntityRef ammoBoxRef, bool andDequeue = false) {
      return TryFindItemInTrash(f, static (p, asset) => asset is AmmoBoxItemAsset ammo && p.target.IsAttachmentAllowed(ammo), 
        out ammoBoxRef, andDequeue, payload: (target: targetWeapon, 0));
    }

    public bool TryFindRebirthTicketInTrash(Frame f, out EntityRef ticketItem, bool andDequeue = false) {
      return TryFindItemInTrash(f, static (_, asset) => asset is RebirthTicketItemAsset, out ticketItem, andDequeue, 0);
    }
    
    bool TryFindItemInTrash<TPayload>(Frame f, Func<TPayload, ItemAsset, bool> predicate, out EntityRef foundItem, bool andDequeue,
      TPayload payload = default)
      where TPayload : struct {
      var items = GetTrashItems(f);

      for (var i = 0; i < items.Count; i++) {
        var itemEntity = items[i];
        var item       = f.Get<Item>(itemEntity);
        var itemAsset  = f.FindAsset(item.Asset);

        if (predicate(payload, itemAsset)) {
          foundItem = itemEntity;

          if (andDequeue) {
            items.RemoveAt(i);
          }

          return true;
        }
      }

      foundItem = EntityRef.None;
      return false;
    }
  }
}