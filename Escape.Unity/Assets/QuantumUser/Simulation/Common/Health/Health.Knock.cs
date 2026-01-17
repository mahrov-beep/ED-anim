namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct Health {
    public void ForceDeath(Frame f, EntityRef source) {
      CurrentValue = FP._0;
      SetupUnitDeadComponent(f, source);
    }
  }
}
