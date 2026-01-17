namespace Quantum {
  using System;

  public static class LoadoutGuidsHelper {
    public static void AssignRandomDeterministicGuids(GameSnapshotLoadout loadout, Guid baseGuid) {
      AssignGuidsForSlots();
      AssignGuidsForTrash();

      void AssignGuidsForSlots() {
        if (loadout.SlotItems == null) {
          return;
        }

        for (var itemIndex = 0; itemIndex < loadout.SlotItems.Length; itemIndex++) {
          var loadoutItem = loadout.SlotItems[itemIndex];
          if (loadoutItem == null) {
            continue;
          }

          AssignRandomDeterministicGuids(loadoutItem, DeterministicGuid.Create(baseGuid, $"Slot:{itemIndex}"));
        }
      }

      void AssignGuidsForTrash() {
        if (loadout.TrashItems == null) {
          return;
        }

        for (var itemIndex = 0; itemIndex < loadout.TrashItems.Length; itemIndex++) {
          var trashItem = loadout.TrashItems[itemIndex];
          if (trashItem == null) {
            continue;
          }

          AssignRandomDeterministicGuids(trashItem, DeterministicGuid.Create(baseGuid, $"Trash:{itemIndex}"));
        }
      }
    }

    public static void AssignRandomDeterministicGuids(GameSnapshotLoadoutItem loadoutItem, Guid baseGuid) {
      loadoutItem.ItemGuid = baseGuid.ToString();

      AssignGuidsForWeaponAttachments();

      void AssignGuidsForWeaponAttachments() {
        if (loadoutItem.WeaponAttachments == null) {
          return;
        }

        for (var ind = 0; ind < loadoutItem.WeaponAttachments.Length; ind++) {
          var attachment = loadoutItem.WeaponAttachments[ind];

          if (attachment == null) {
            continue;
          }

          AssignRandomDeterministicGuids(attachment, DeterministicGuid.Create(namespaceId: baseGuid, name: $"Attachment:{ind}"));
        }
      }
    }

    public static void AssignRandomDeterministicGuids(GameSnapshotLoadoutWeaponAttachment weaponAttachment, Guid baseGuid) {
      weaponAttachment.ItemGuid = baseGuid.ToString();
    }
  }
}