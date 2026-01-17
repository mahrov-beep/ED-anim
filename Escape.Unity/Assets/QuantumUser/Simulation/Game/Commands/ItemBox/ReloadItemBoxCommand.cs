namespace Quantum.Commands {
  using Photon.Deterministic;
  public unsafe class ReloadItemBoxCommand : CharacterCommandBase {
    public override void Serialize(BitStream stream) { }

    public override void Execute(Frame f, EntityRef characterEntity) {
      var storageData = ReloadItemBoxCommandHelper.GetPendingStorageData();
      if (storageData == null) {
        f.LogWarning(characterEntity, "ReloadItemBoxCommand: no pending storage data");
        return;
      }

      var unit = f.GetPointer<Unit>(characterEntity);

      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        f.LogWarning(characterEntity, "ReloadItemBoxCommand: no CharacterLoadout component");
        return;
      }

      if (unit->NearbyItemBox != EntityRef.None) {
        f.Destroy(unit->NearbyItemBox);
      }

      var itemBoxEntity = f.Global->CreateItemBox(f, f.Get<Transform3D>(characterEntity).Position,
        autoUnpackNestedItems: false,
        keelAliveWithoutItems: true
      );

      var itemBox = f.GetPointer<ItemBox>(itemBoxEntity);
      itemBox->Width  = storageData.Width;
      itemBox->Height = storageData.Height;
      
      itemBox->CreateFromRuntimeStorage(f, storageData.Storage);

      itemBox->OpenerUnitRef = characterEntity;
      unit->NearbyItemBox = itemBoxEntity;

      if (itemBox->Width > 0 && itemBox->Height > 0) {
        loadout->LoadStorageItems(f, itemBoxEntity);
      }

      ReloadItemBoxCommandHelper.ClearPendingStorageData();
    }
  }
}

