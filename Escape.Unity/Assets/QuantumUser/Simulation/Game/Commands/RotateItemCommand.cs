namespace Quantum.Commands {
  using Photon.Deterministic;

  public class RotateItemCommand : CharacterCommandBase {
    public EntityRef ItemEntity;
    public byte      Source;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.ItemEntity);
      stream.Serialize(ref this.Source);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      unsafe {
        if (!f.Exists(this.ItemEntity)) {
          Log.Error("RotateItemCommand: itemEntity not exist");
          return;
        }

        if (!f.TryGetPointer(this.ItemEntity, out Item* item)) {
          Log.Error("RotateItemCommand: no Item component on itemEntity");
          return;
        }

        if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
          Log.Error("RotateItemCommand: loadout not exist");
          return;
        }

        if (!loadout->HasItemInTrash(f, this.ItemEntity, this.Source)) {
          Log.Error("RotateItemCommand: item not in loadout");
          return;
        }

        var (i, j) = (item->IndexI, item->IndexJ);

        if (!loadout->RemoveItemFromTetris(f, this.ItemEntity, this.Source)) {
          Log.Error("RotateItemCommand: cannot temporarily remove item from trash");
          return;
        }

        var newRotation = item->Rotated ? RotationType.Default : RotationType.Rotated;

        if (loadout->CanBePlaceIn(f, this.ItemEntity, i, j, out var place, newRotation, this.Source)) {
          loadout->AddItemToTrash(f, this.ItemEntity, place.I, place.J, place.Rotated);
        }
        else {
          loadout->AddItemToTrash(f, this.ItemEntity, i, j, item->Rotated);
        }
      }
    }
  }
}