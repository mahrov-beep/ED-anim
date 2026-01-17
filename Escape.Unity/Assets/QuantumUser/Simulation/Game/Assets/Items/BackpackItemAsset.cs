namespace Quantum {
  using System;

  [Serializable]
  public unsafe class BackpackItemAsset : ItemAsset {
    public override ItemTypes ItemType => ItemTypes.Backpack;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Equipment;

    public override void Reset() {
      base.Reset();

      this.validSlots = new[] { CharacterLoadoutSlots.Backpack };
    }
  }
}