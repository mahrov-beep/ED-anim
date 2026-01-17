namespace Quantum {
  using System;

  [Serializable]
  public class SkinItemAsset : ItemAsset {
    public AssetRef<EntityPrototype> characterPrototype;

    public override ItemTypes ItemType => ItemTypes.Skin;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Skins;

    public override void Reset() {
      base.Reset();

      this.validSlots = new[] { CharacterLoadoutSlots.Skin };
    }
  }
}