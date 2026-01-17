namespace Quantum {
  using Sirenix.OdinInspector;

  public class SafeItemAsset : ItemAsset {
    [PropertySpace(SpaceBefore = 10)]
    [InfoBox("Safe grid size (Tetris) defined by this item.")]
    public int SafeWidth = 4;

    public int SafeHeight = 4;

    public override ItemTypes ItemType => ItemTypes.Safe;
    
    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Equipment;
    
    public override void Reset() {
      base.Reset();

      this.validSlots = new[] { CharacterLoadoutSlots.Safe };
    }
  }
}


