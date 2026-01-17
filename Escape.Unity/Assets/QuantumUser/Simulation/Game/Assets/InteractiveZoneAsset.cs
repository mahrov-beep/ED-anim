namespace Quantum {
  using System;
  using Photon.Deterministic;

  [Serializable]
  public abstract class InteractiveZoneAsset : AssetObject {
    public FP interactionTime = 1;

    public virtual bool CanInteract(Frame f, EntityRef zoneEntity, EntityRef beneficiaryUnitEntity) => true;
    public abstract void OnInteractComplete(Frame f, EntityRef zoneEntity, EntityRef beneficiaryUnitEntity);
  }
}