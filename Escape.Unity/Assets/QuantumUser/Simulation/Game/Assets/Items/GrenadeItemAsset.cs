namespace Quantum {
  using System;

  [Serializable]
  public class GrenadeItemAsset : ItemAsset {
    public override ItemTypes ItemType => ItemTypes.Grenade;
  }
}