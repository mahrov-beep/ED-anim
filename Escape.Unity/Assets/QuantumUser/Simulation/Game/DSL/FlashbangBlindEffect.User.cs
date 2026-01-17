namespace Quantum {
  
  using Photon.Deterministic;
  public unsafe partial struct FlashbangBlindEffect
  {
    public bool IsActive(Frame f)
    {
      var currentTime = f.Number;
      var elapsedTime = (currentTime - StartTime) * f.DeltaTime;
      return elapsedTime < Duration;
    }

    public FP GetCurrentStrength(Frame f)
    {
      if (!IsActive(f)) return FP._0;

      var currentTime = f.Number;
      var elapsedTime = (currentTime - StartTime) * f.DeltaTime;
      var normalizedTime = elapsedTime / Duration;
      
      return Strength * (FP._1 - normalizedTime);
    }
  }
}
