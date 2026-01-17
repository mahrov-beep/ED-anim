namespace Quantum.Commands {
  using Photon.Deterministic;

  public unsafe class CloseItemBoxCommand : CharacterCommandBase {

    public override void Serialize(BitStream stream) {
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        f.LogWarning(characterEntity, "CloseItemBoxCommand: not found loadout");

        return;
      }
      
      var nearbyItemBoxRef = loadout->StorageEntity;

      var storageItems = loadout->GetStorageItems(f);
      loadout->StorageEntity = EntityRef.None;

      if (!f.Exists(nearbyItemBoxRef)) {
        f.LogWarning(characterEntity, "CloseItemBoxCommand: no nearby ItemBox");
        return;
      }

      var itemBox = f.GetPointer<ItemBox>(nearbyItemBoxRef);

      Log.Info($"CloseItemBoxCommand: Unloaded {storageItems.Count} items from ItemBox");
      
      if (itemBox->OpenerUnitRef == EntityRef.None) {
        f.LogWarning(characterEntity, "CloseItemBoxCommand: itemBox already closed");
        return;
      }

      var itemRefs = f.ResolveList(itemBox->ItemRefs);

      if (itemRefs.Count == 0 && (itemBox->IsBackpack || f.Has<DropFromUnitMarker>(itemBox->SelfItemBoxEntity))) {
        f.Destroy(itemBox->SelfItemBoxEntity);
        return;
      }

      itemBox->OpenerUnitRef = EntityRef.None;
    }
  }
}