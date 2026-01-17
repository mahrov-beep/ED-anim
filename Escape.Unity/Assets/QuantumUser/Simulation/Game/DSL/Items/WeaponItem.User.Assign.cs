namespace Quantum {
  using System;
  using AssignOptions = CharacterLoadout.AssignOptions;
  using UnassignOptions = CharacterLoadout.UnassignOptions;

  public unsafe partial struct WeaponItem {
    public bool HasAnyAttachment(Frame f) {
      foreach (var weaponSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
        if (AttachmentAtSlot(weaponSlot) != EntityRef.None) {
          return true;
        }
      }

      return false;
    }

    public ref EntityRef AttachmentAtSlot(WeaponAttachmentSlots slot) => ref this.WeaponAttachmentsRaw[(int)slot];

    public bool HasAttachmentAtSlot(WeaponAttachmentSlots slot) {
      return AttachmentAtSlot(slot) is var attachmentEntity && attachmentEntity != EntityRef.None;
    }

    public bool CanAssignAttachmentToSlot(Frame f, WeaponAttachmentSlots slot, EntityRef newAttachmentEntity,
      AssignOptions options = AssignOptions.None) {
      return AssignAttachmentToSlot(f, slot, newAttachmentEntity, realRun: false, options);
    }

    public bool AssignAttachmentToSlot(Frame f, WeaponAttachmentSlots slot, EntityRef newAttachmentEntity) {
      return AssignAttachmentToSlot(f, slot, newAttachmentEntity, realRun: true, AssignOptions.None);
    }

    public bool CanUnassignAttachmentFromSlot(Frame f, WeaponAttachmentSlots slot, EntityRef oldAttachmentEntity) {
      return UnassignAttachmentFromSlot(f, slot, oldAttachmentEntity, realRun: false);
    }

    public bool UnassignAttachmentFromSlot(Frame f, WeaponAttachmentSlots slot, EntityRef oldAttachmentEntity) {
      return UnassignAttachmentFromSlot(f, slot, oldAttachmentEntity, realRun: true);
    }

    bool AssignAttachmentToSlot(Frame f, WeaponAttachmentSlots slot, EntityRef newAttachmentEntity, bool realRun, AssignOptions options) {
      if (slot == WeaponAttachmentSlots.Invalid) {
        if (realRun) {
          Log.Error("Trying to assign weapon attachment to Invalid slot");
        }

        return false;
      }

      if (newAttachmentEntity == EntityRef.None) {
        if (realRun) {
          Log.Error("Trying to assign EntityRef.None weapon attachment");
        }

        return false;
      }

      if ((options & AssignOptions.SKipSlotAlreadyAssignedCheck) == 0 && this.HasAttachmentAtSlot(slot)) {
        if (realRun) {
          Log.Error("Trying to assign weapon attachment to non empty slot");
        }

        return false;
      }

      if (!f.TryGetPointer(SelfWeaponEntity, out Item* selfItem)) {
        if (realRun) {
          Log.Error("Failed to get Item from self entity");
        }

        return false;
      }

      if (f.FindAsset(selfItem->Asset) is not WeaponItemAsset weaponItemAsset) {
        if (realRun) {
          Log.Error("WeaponItem asset is not WeaponItemAsset");
        }

        return false;
      }

      if (Array.IndexOf(weaponItemAsset.attachmentsSchema?.slots ?? Array.Empty<WeaponAttachmentSlots>(), slot) == -1) {
        if (realRun) {
          Log.Error("Trying to assign weapon attachment to slot non valid by attachmentsSchema");
        }

        return false;
      }

      if (f.TryGetPointer(selfItem->Owner, out CharacterLoadout* loadout)) {
        if (!loadout->HasEnoughFreeSpaceForItem(f, newAttachmentEntity)) {
          if (realRun) {
            Log.Error("Trying to assign item that will overflow weight limit");
          }

          return false;
        }
      }

      var newItem      = f.Get<Item>(newAttachmentEntity);
      var newItemAsset = f.FindAsset(newItem.Asset);

      if (newItemAsset is WeaponAttachmentItemAsset weaponAttachmentItemAsset &&
          !weaponItemAsset.IsAttachmentAllowed(weaponAttachmentItemAsset)) {
        if (realRun) {
          Log.Error("Trying to assign not allowed weapon attachments");
        }

        return false;
      }

      if (!newItemAsset.CanBeAssignedToSlot(f, this.SelfWeaponEntity, newAttachmentEntity, 
            CharacterLoadoutSlots.Invalid, slot, out var reason)) {
        if (realRun) {
          Log.Error($"Cannot assign attachment to target weapon slot: {reason}");
        }

        return false;
      }

      if (realRun) {
        this.AttachmentAtSlot(slot) = newAttachmentEntity;
        newItemAsset.ChangeItemOwner(f, newAttachmentEntity, this.SelfWeaponEntity);

        NotifyLoadoutModified(f, newAttachmentEntity);
      }

      return true;
    }

    bool UnassignAttachmentFromSlot(Frame f, WeaponAttachmentSlots slot, EntityRef oldAttachmentEntity, bool realRun) {
      if (oldAttachmentEntity == EntityRef.None) {
        if (realRun) {
          Log.Error("Trying to unassing Entity.None item from weapon attachment slot");
        }

        return false;
      }

      if (slot == WeaponAttachmentSlots.Invalid) {
        if (realRun) {
          Log.Error("Trying to unassign item from Invalid weapon attachment slot");
        }

        return false;
      }

      if (this.AttachmentAtSlot(slot) != oldAttachmentEntity) {
        if (realRun) {
          Log.Error("Trying to unassign mismatched entity from weapon attachment slot");
        }

        return false;
      }

      var oldItem      = f.Get<Item>(oldAttachmentEntity);
      var oldItemAsset = f.FindAsset(oldItem.Asset);

      if (!oldItemAsset.CanBeUnAssignedFromSlot(f, oldAttachmentEntity, CharacterLoadoutSlots.Invalid, slot, out var reason)) {
        if (realRun) {
          Log.Error($"Cannot unassign attachment from target weapon slot: {reason}");
        }

        return false;
      }

      if (realRun) {
        if (slot == WeaponAttachmentSlots.Ammo) {
          Weapon.UnloadAmmoFromWeaponToAmmoBox(f, SelfWeaponEntity);
        }

        this.AttachmentAtSlot(slot) = EntityRef.None;
        oldItemAsset.ChangeItemOwner(f, oldAttachmentEntity, EntityRef.None);
        NotifyLoadoutModified(f, oldAttachmentEntity);
      }

      return true;
    }

    public bool MoveAttachmentToLoadoutTrash(Frame f, WeaponAttachmentSlots slot, CharacterLoadout* loadout, int indexI, int indexJ, bool rotated, byte source) {
      var attachmentEntity = AttachmentAtSlot(slot);

      if (attachmentEntity == EntityRef.None) {
        Log.Error("Trying to move Entity.None item from weapon slot to trash");
        return false;
      }

      if (!UnassignAttachmentFromSlot(f, slot, attachmentEntity)) {
        return false;
      }

      if (!loadout->AddItemToTrash(f, attachmentEntity, indexI, indexJ, rotated, source: source)) {
        AssignAttachmentToSlot(f, slot, attachmentEntity);
        return false;
      }

      return true;
    }

    public void NotifyLoadoutModified(Frame f, EntityRef itemRef) {
      if (f.TryGetPointer(SelfWeaponEntity, out Item* selfItem) &&
          f.TryGetPointer(selfItem->Owner, out CharacterLoadout* loadout)) {
        loadout->NotifyLoadoutModified(f, itemRef, this.SelfWeaponEntity);
      }
    }
  }
}