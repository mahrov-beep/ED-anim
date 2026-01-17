namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Unity.Cinemachine;

  [Serializable]
  public class CinemachineImpulseAsset : AssetObject {
    public FPVector3 impulsePower = FPVector3.Forward;

    public CinemachineImpulseDefinition impulse = new CinemachineImpulseDefinition();
  }
}