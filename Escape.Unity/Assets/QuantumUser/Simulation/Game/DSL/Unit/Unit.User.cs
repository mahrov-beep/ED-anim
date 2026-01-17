namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct Unit {
    public static ComponentHandler<Unit> InitRNG => static (f, e, c) => {
      c->RNG = new RNGSession();
    };    
    
    public static ComponentHandler<Unit> PrepareSlowDebuff => static (f, e, c) => {
      f.Set(e, new SlowDebuff());
    };

    public FP CurrentSpeedCoefficient {
      get {
        var maxSpeed = CurrentStats.moveSpeed.AsFP;
        return maxSpeed < FP._0_01 ? FP._1 : FPMath.Clamp01(CurrentSpeed / maxSpeed);
      }
    }
  }
}