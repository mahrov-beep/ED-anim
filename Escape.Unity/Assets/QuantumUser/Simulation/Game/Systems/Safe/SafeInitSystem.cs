namespace Quantum {
  public unsafe class SafeInitSystem : SystemSignalsOnly,
    ISignalOnComponentAdded<CharacterLoadout>,
    ISignalOnCharacterAfterLoadoutSlotAssigned,
    ISignalOnCharacterBeforeLoadoutSlotUnassigned {

    public void OnAdded(Frame f, EntityRef unitEntity, CharacterLoadout* loadout) {
      if (f.TryGetPointer<CharacterSafe>(unitEntity, out var safeContainer)) {
        SetSizeFromSafeSlot(f, unitEntity, loadout, safeContainer);
        return;
      }

      f.Add(unitEntity, out safeContainer);
      safeContainer->SelfUnitEntity = unitEntity;

      SetSizeFromSafeSlot(f, unitEntity, loadout, safeContainer);
    }

    public void OnCharacterAfterLoadoutSlotAssigned(Frame f, EntityRef characterEntity, CharacterLoadoutSlots slot, EntityRef itemEntity) {
      Log.Info($"SafeInitSystem.OnCharacterAfterLoadoutSlotAssigned: slot={slot}, itemEntity={itemEntity}");
      if (!f.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        return;
      }
      
      if (slot == CharacterLoadoutSlots.Safe && f.TryGetPointer<CharacterSafe>(characterEntity, out var safeContainer)) {
        SetSizeFromSafeSlot(f, characterEntity, loadout, safeContainer);
        return;
      }
    }

    public void OnCharacterBeforeLoadoutSlotUnassigned(Frame f, EntityRef characterEntity, CharacterLoadoutSlots slot,
      EntityRef itemEntity) {
      Log.Info($"SafeInitSystem.OnCharacterBeforeLoadoutSlotUnassigned: slot={slot}, itemEntity={itemEntity}");
      if (!f.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        return;
      }
      
      if (slot == CharacterLoadoutSlots.Safe && f.TryGetPointer<CharacterSafe>(characterEntity, out var safeContainer)) {
        SetSizeFromSafeSlot(f, characterEntity, loadout, safeContainer);
        return;
      }
    }

    private static void SetSizeFromSafeSlot(Frame f, EntityRef unitEntity, CharacterLoadout* loadout, CharacterSafe* safeContainer) {
      int width  = 4;
      int height = 4;

      var safeSlot = CharacterLoadoutSlots.Safe;
      var itemRef  = loadout->ItemAtSlot(safeSlot);

      if (itemRef != EntityRef.None) {
        var item = f.Get<Item>(itemRef);

        if (f.FindAsset(item.Asset) is SafeItemAsset asset) {
          width  = asset.SafeWidth;
          height = asset.SafeHeight;
        }
      }

      safeContainer->Width  = width;
      safeContainer->Height = height;

      // Генерируем сигнал изменения размера сейфа
      f.Signals.OnSafeResized(unitEntity, width, height);
    }
  }
}


