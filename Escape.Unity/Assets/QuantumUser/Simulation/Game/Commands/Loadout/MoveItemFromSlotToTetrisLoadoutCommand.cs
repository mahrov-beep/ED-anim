namespace Quantum.Commands {
  using Photon.Deterministic;

  public class MoveItemFromSlotToTetrisLoadoutCommand : CharacterCommandBase {
    public CharacterLoadoutSlots OldSlotType;
    public EntityRef             ItemEntity;
    public int                   IndexI;
    public int                   IndexJ;
    public bool                  Rotated;
    public byte                  DestinationSource;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.OldSlotType);
      stream.Serialize(ref this.ItemEntity);
      stream.Serialize(ref this.IndexI);
      stream.Serialize(ref this.IndexJ);
      stream.Serialize(ref this.Rotated);
      stream.Serialize(ref this.DestinationSource);
    }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("MoveItemFromSlotToTrash: loadout not exist");
        return;
      }

      if (!loadout->UnassignItemFromSlot(f, this.OldSlotType, this.ItemEntity)) {
        Log.Error("MoveItemFromSlotToTrash: failed to unassign item from old slot");
        return;
      }

      if (!loadout->AddItemToTrash(f, this.ItemEntity, this.IndexI, this.IndexJ, this.Rotated, null, this.DestinationSource)) {
        Log.Error("MoveItemFromSlotToTrash: failed to add item to trash");

        loadout->AssignItemToSlot(f, this.OldSlotType, this.ItemEntity); // возвращаем обратно если ошибка
        return;
      }
    }
  }
}