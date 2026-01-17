namespace Quantum {
  using System;

  [Serializable]
  public class ArmorItemAsset : ItemAsset {
    public override ItemTypes ItemType => ItemTypes.Armor;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Equipment;

    public override void Reset() {
      base.Reset();

      this.validSlots = new[] { CharacterLoadoutSlots.Armor };
    }
  }
}