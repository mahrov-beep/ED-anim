namespace Quantum {
  using System;
  using Photon.Deterministic;

  public partial class RuntimeConfig {
    public string                  DeterministicGuidNamespace;
    public AssetRef<GameModeAsset> GameModeAsset;

    public Guid DeterministicGuid => Guid.Parse(DeterministicGuidNamespace);

    partial void SerializeUserData(BitStream stream) {
      stream.Serialize(ref DeterministicGuidNamespace);
      stream.Serialize(ref GameModeAsset);
    }
  }
}