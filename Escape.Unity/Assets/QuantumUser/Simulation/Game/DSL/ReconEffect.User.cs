namespace Quantum {
  using Photon.Deterministic;
  
  public unsafe partial struct ReconEffect {
    public bool IsEntityInRange(FPVector3 entityPosition) {
      var distance = FPVector3.Distance(Position, entityPosition);
      return distance <= Radius;
    }
  }
}

