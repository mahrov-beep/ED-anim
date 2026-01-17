namespace Quantum.Commands {
  using Photon.Deterministic;

  public class UseItemCommand : CharacterCommandBase {
    public EntityRef ItemEntity;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.ItemEntity);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Exists(this.ItemEntity)) {
        Log.Error("UseItemCommand: itemEntity not exist");
        return;
      }

      if (!f.TryGet(this.ItemEntity, out Item item)) {
        Log.Error("UseItemCommand: no Item component on itemEntity");
        return;
      }

      if (item.Owner != characterEntity) {
        Log.Error("UseItemCommand: failed to use not owned item");
        return;
      }

      var itemAsset = f.FindAsset(item.Asset);

      if (itemAsset is not UsableItemAsset usableItemAsset) {
        Log.Error("UseItemCommand: item asset is not UsableItemAsset");
        return;
      }

      usableItemAsset.UseItem(f, this.ItemEntity, characterEntity);
    }
  }
}