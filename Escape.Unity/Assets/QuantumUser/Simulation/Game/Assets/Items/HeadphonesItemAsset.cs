namespace Quantum {
  using System;

  [Serializable]
  public class HeadphonesItemAsset : ItemAsset {
    public override ItemTypes ItemType => ItemTypes.Headphones;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Equipment;

    public override void Reset() {
      base.Reset();

      this.validSlots = new[] { CharacterLoadoutSlots.Headphones };
    }
  }
}