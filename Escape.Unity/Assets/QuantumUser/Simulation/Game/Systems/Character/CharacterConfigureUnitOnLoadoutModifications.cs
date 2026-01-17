namespace Quantum {
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class CharacterConfigureUnitOnLoadoutModifications : SystemSignalsOnly,
    ISignalOnCharacterBeforeLoadoutSlotUnassigned,
    ISignalOnCharacterAfterLoadoutSlotAssigned {
    public void OnCharacterBeforeLoadoutSlotUnassigned(Frame f,
      EntityRef characterEntity, CharacterLoadoutSlots slot, EntityRef itemEntity) {

      var unit = f.Unsafe.GetPointer<Unit>(characterEntity);

      Log.Info($"OnCharacterBeforeLoadoutSlotUnassigned: slot={slot}, itemEntity={itemEntity}");

      switch (slot) {
        case CharacterLoadoutSlots.PrimaryWeapon:
          unit->PrimaryWeapon = EntityRef.None;
          break;
        case CharacterLoadoutSlots.SecondaryWeapon:
          unit->SecondaryWeapon = EntityRef.None;
          break;
        case CharacterLoadoutSlots.MeleeWeapon:
          unit->MeleeWeapon = EntityRef.None;
          break;

        case CharacterLoadoutSlots.Safe:
          UpdateSafeFromSlot(f, characterEntity, EntityRef.None);
          break;

        default:
          Log.Warn($"Unhandled slot in OnCharacterBeforeLoadoutSlotUnassigned: {slot}");
          break;
      }
      
      if (unit->ActiveWeaponRef == itemEntity) {
        unit->TryChangeWeapon(f, unit->ValidWeaponRef, allowNull: true);
      }
    }

    public void OnCharacterAfterLoadoutSlotAssigned(Frame f,
      EntityRef characterEntity, CharacterLoadoutSlots slot, EntityRef itemEntity) {
      var unit = f.Unsafe.GetPointer<Unit>(characterEntity);

      Log.Info($"OnCharacterAfterLoadoutSlotAssigned: slot={slot}, itemEntity={itemEntity}");

      switch (slot) {
        case CharacterLoadoutSlots.PrimaryWeapon:
          unit->PrimaryWeapon = itemEntity;
          unit->TryChangeWeapon(f, itemEntity);
          break;

        case CharacterLoadoutSlots.SecondaryWeapon:
          unit->SecondaryWeapon = itemEntity;
          unit->TryChangeWeapon(f, itemEntity);
          break;

        case CharacterLoadoutSlots.MeleeWeapon:
          unit->MeleeWeapon = itemEntity;
          unit->TryChangeWeapon(f, itemEntity);
          break;

        case CharacterLoadoutSlots.Skill:
          unit->AbilityRef = itemEntity;
          break;

        case CharacterLoadoutSlots.Skin:
          unit->Skin = itemEntity;
          break;

        case CharacterLoadoutSlots.Safe:
          UpdateSafeFromSlot(f, characterEntity, itemEntity);
          break;

        default:
          Log.Warn($"Unhandled slot in OnCharacterAfterLoadoutSlotAssigned: {slot}");
          break;
      }
    }

    private static void UpdateSafeFromSlot(Frame f, EntityRef characterEntity, EntityRef safeItemEntity) {
      if (!f.TryGetPointer<CharacterSafe>(characterEntity, out var safe)) {
        f.Add(characterEntity, out safe);
        
        safe->SelfUnitEntity = characterEntity;
      }

      int width = 4;
      int height = 4;

      if (safeItemEntity != EntityRef.None) {
        var item = f.Get<Item>(safeItemEntity);
        
        if (f.FindAsset(item.Asset) is SafeItemAsset asset) {
          width = asset.SafeWidth;
          height = asset.SafeHeight;
        } else {
          Log.Warn($"Item {item.Asset} is not a SafeItemAsset");
        }
      } else {
        Log.Info("No safe item, using default size");
      }

      safe->Width = width;
      safe->Height = height;
    }
  }
}