namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Prototypes;
  using Sirenix.OdinInspector;

  [Serializable]
  public unsafe class HealBoxItemAsset : UsableItemAsset {
    [InlineProperty(LabelWidth = 100)]
    public HealthApplicatorPrototype heal;

    [InlineProperty(LabelWidth = 100)]
    public FP hideWeaponsDelaySeconds = FP._0_50;

    public override ItemTypes ItemType => ItemTypes.HealBox;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Health;

    public override bool CanBeUsed(Frame f, EntityRef itemEntity) {
      if (!base.CanBeUsed(f, itemEntity)) {
        return false;
      }

      if (!f.TryGet(itemEntity, out Item item)) {
        return false;
      }

      if (!f.TryGetPointer(item.Owner, out Health* health)) {
        return false;
      }

      if (health->IsFull) {
        return false;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, item.Owner)) {
        return false;
      }

      return true;
    }

    public override void UseItem(Frame f, EntityRef itemEntity, EntityRef unitEntity) {   
      var healingState = new CharacterStateHealing {
        Duration = this.useDurationSeconds,
        Timer = this.useDurationSeconds,
        ItemEntity = itemEntity,
      };

      var applicator = new HealthApplicator();
      this.heal.Materialize(f, ref applicator);
      healingState.Applicator = applicator;

      if (!CharacterFsm.TryEnterState(f, unitEntity, healingState)) {
        return;
      }

      base.UseItem(f, itemEntity, unitEntity);
    }
  }
}