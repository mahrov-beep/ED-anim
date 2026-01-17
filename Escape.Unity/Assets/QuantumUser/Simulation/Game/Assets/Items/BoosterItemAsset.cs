namespace Quantum {
  using System;

  [Serializable]
  public unsafe class BoosterItemAsset : UsableItemAsset {
    public override ItemTypes ItemType => ItemTypes.Booster;

    public override bool CanBeUsed(Frame f, EntityRef itemEntity) {
      if (!base.CanBeUsed(f, itemEntity)) {
        return false;
      }

      if (!f.TryGet(itemEntity, out Item item)) {
        return false;
      }

      if (!f.Has<Unit>(item.Owner)) {
        return false;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, item.Owner)) {
        return false;
      }

      return true;
    }
  }
}