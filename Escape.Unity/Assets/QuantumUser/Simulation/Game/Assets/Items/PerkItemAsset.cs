namespace Quantum {
  using System;

  [Serializable]
  public class PerkItemAsset : ItemAsset {
    public override ItemTypes ItemType => ItemTypes.Perk;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Perks;

    public override void Reset() {
      base.Reset();

      this.validSlots = new[] { CharacterLoadoutSlots.Perk1, CharacterLoadoutSlots.Perk2, CharacterLoadoutSlots.Perk3 };
    }
  }
}