namespace Quantum.Commands {
  using Photon.Deterministic;
  public unsafe class OpenItemBoxCommand : CharacterCommandBase {
    public bool OpenBackpack;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.OpenBackpack);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        return;
      }

      if (loadout->StorageEntity != EntityRef.None) {
        new CloseItemBoxCommand().Execute(f, characterEntity);
      }

      var openerUnit = f.GetPointer<Unit>(characterEntity);
      var nearbyItemBoxRef = this.OpenBackpack ? openerUnit->NearbyBackpack : openerUnit->NearbyItemBox;

      if (nearbyItemBoxRef == EntityRef.None) {
        f.LogWarning(characterEntity, $"OpenItemBoxCommand: no nearby {(this.OpenBackpack ? "Backpack" : "ItemBox")}");
        return;
      }

      var itemBox = f.GetPointer<ItemBox>(nearbyItemBoxRef);

      if (itemBox->OpenerUnitRef != EntityRef.None && itemBox->OpenerUnitRef != characterEntity) {
        f.LogWarning(characterEntity, $"OpenItemBoxCommand: {(this.OpenBackpack ? "backpack" : "itemBox")} already opened by another unit");
        return;
      }
      
      if (f.Has<TimerItemBoxMarker>(itemBox->SelfItemBoxEntity)) {
        return;
      }
      
      if (itemBox->TimerToOpen > 0) {
        f.Set(itemBox->SelfItemBoxEntity, new TimerItemBoxMarker());
        return;
      }
      
      itemBox->OpenerUnitRef = characterEntity;

      loadout->LoadStorageItems(f, itemBox->SelfItemBoxEntity);
      
      if (f.Has<OpenedItemBoxMarker>(itemBox->SelfItemBoxEntity)) {
        return;
      }
      
      f.Set(itemBox->SelfItemBoxEntity, new OpenedItemBoxMarker());

      f.Signals.OnOpenItemBox(itemBox->SelfItemBoxEntity);
      f.Events.OpenItemBox(itemBox->SelfItemBoxEntity);
    }
  }
}