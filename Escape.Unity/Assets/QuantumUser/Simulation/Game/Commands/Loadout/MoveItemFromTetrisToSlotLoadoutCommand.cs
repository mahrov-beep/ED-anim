namespace Quantum.Commands {
  using Photon.Deterministic;


  public class MoveItemFromTetrisToSlotLoadoutCommand : CharacterCommandBase {
    public CharacterLoadoutSlots NewSlotType;
    public EntityRef             ItemEntity;
    public byte                  FromSource;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.NewSlotType);
      stream.Serialize(ref this.ItemEntity);
      stream.Serialize(ref this.FromSource);
    }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("MoveItemFromTrashToSlot: loadout not exist");
        return;
      }

      if (!loadout->RemoveItemFromTetris(f, this.ItemEntity, this.FromSource)) {
        Log.Error("MoveItemFromTrashToSlot: failed to remove item from trash");
        return;
      }

      if (!loadout->AssignOrTrashSwapItemToSlot(f, this.NewSlotType, this.ItemEntity, this.FromSource)) {
        Log.Error("MoveItemFromTrashToSlot: failed to remove item from trash");

        var item = f.Get<Item>(this.ItemEntity);
        
        loadout->AddItemToTrash(f, this.ItemEntity, item.IndexI, item.IndexJ, item.Rotated, null, this.FromSource); // возвращаем обратно если ошибка
        return;
      }
    }
  }
}