namespace Quantum.Commands {
  using Photon.Deterministic;

  public class MoveItemFromSlotToSlotLoadoutCommand : CharacterCommandBase {
    public CharacterLoadoutSlots OldSlotType;
    public CharacterLoadoutSlots NewSlotType;
    public EntityRef             ItemEntity;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.OldSlotType);
      stream.Serialize(ref this.NewSlotType);
      stream.Serialize(ref this.ItemEntity);
    }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("MoveItemFromSlotToSlot: loadout not exist");
        return;
      }

      if (!loadout->UnassignItemFromSlot(f, this.OldSlotType, this.ItemEntity)) {
        Log.Error("MoveItemFromSlotToSlot: failed to unassign item from old slot");
        return;
      }

      // случай если мы меняем предметы между слотами, например, Primary и Secondary оружие
      var canSwapBetweenSlots = loadout->TryGetItemAtSlot(this.NewSlotType, out var otherItemRef) &&
                                loadout->CanUnassignItemFromSlot(f, this.NewSlotType, otherItemRef) &&
                                loadout->CanAssignItemToSlot(f, this.OldSlotType, otherItemRef);
      if (canSwapBetweenSlots) {
        loadout->UnassignItemFromSlot(f, this.NewSlotType, otherItemRef);
        loadout->AssignItemToSlot(f, this.OldSlotType, otherItemRef);
      }

      if (!loadout->AssignOrTrashSwapItemToSlot(f, this.NewSlotType, this.ItemEntity, 0)) {
        Log.Error("MoveItemFromSlotToSlot: failed to assign item to new slot");

        loadout->AssignItemToSlot(f, this.OldSlotType, this.ItemEntity); // возвращаем обратно если ошибка
        return;
      }
    }
  }
}