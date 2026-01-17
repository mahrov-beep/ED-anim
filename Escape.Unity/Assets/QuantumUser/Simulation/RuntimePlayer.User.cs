// ReSharper disable UnassignedField.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable InconsistentNaming

namespace Quantum {
  using JetBrains.Annotations;

  public partial class RuntimePlayer {
    [CanBeNull] public string              NickName;
    [CanBeNull] public GameSnapshotLoadout Loadout;
    [CanBeNull] public GameSnapshotStorage Storage;

    public int   StorageWidth;
    public int   StorageHeight;
    public ulong PartyKeyHash;
  }
}