namespace Quantum {
using Photon.Deterministic;
public static class FPExtensions {
  public static bool ProcessTimer(this ref FP timer, Frame f) {
    timer = FPMath.Max(0, timer - f.DeltaTime);
    
    return timer <= FP._0;
  }  
  
  public static bool ProcessTimer(this ref FP timer, FP deltaTime) {
    timer = FPMath.Max(0, timer - deltaTime);

    return timer <= FP._0;
  }

  public static bool IsTimerExpired(this FP timer) => timer <= FP._0;
}
}