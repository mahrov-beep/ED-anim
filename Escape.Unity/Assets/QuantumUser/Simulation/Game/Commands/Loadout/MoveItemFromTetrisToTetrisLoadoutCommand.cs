namespace Quantum.Commands {
  using Photon.Deterministic;

  public class MoveItemFromTetrisToTetrisLoadoutCommand : CharacterCommandBase {
    public EntityRef ItemEntity;
    public int       IndexI;
    public int       IndexJ;
    public bool      Rotated;
    public byte      Source;
    public byte      Destination;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.ItemEntity);
      stream.Serialize(ref this.IndexI);
      stream.Serialize(ref this.IndexJ);
      stream.Serialize(ref this.Rotated);
      stream.Serialize(ref this.Source);
      stream.Serialize(ref this.Destination);
    }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("MoveItemFromTetrisToTetris: loadout not exist");
        return;
      }

      if (loadout->TryGetItemAt(f, this.IndexI, this.IndexJ, out var possibleMergeTarget, this.Destination) &&
          loadout->CanMerge(f, this.ItemEntity, possibleMergeTarget)) {
        loadout->Merge(f, this.ItemEntity, possibleMergeTarget, this.Destination);
        return;
      }

      if (!loadout->RemoveItemFromTetris(f, this.ItemEntity, this.Source)) {
        Log.Error("MoveItemFromTetrisToTetris: failed to remove item from tetris");
        return;
      }
      
      if (!loadout->AddItemToTrash(f, this.ItemEntity, this.IndexI, this.IndexJ, this.Rotated, null, this.Destination)) {
        Log.Error("MoveItemFromTetrisToTetris: failed to add item to trash");
        return;
      }
    }
  }
}