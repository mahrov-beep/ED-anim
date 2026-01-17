namespace Quantum {
  using System;

  [Serializable]
  public class HelmetItemAsset : ItemAsset {
    public override ItemTypes ItemType => ItemTypes.Helmet;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Equipment;

    public override void Reset() {
      base.Reset();

      this.validSlots = new[] { CharacterLoadoutSlots.Helmet };
    }
  }
}