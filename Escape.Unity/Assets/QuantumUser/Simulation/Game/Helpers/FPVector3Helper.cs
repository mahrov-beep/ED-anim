namespace Quantum {
  using Photon.Deterministic;

  public static class FPVector3Helper {
    public static FPVector3 SmoothDampUnclamped(FPVector3 current, FPVector3 target, ref FPVector3 currentVelocity, FP smoothTime,
      FP deltaTime) {
      FP _0_48  = (FP)48 / 100;
      FP _0_235 = (FP)235 / 1000;
      FP omega  = FP._2 / smoothTime;
      FP x      = omega * deltaTime;
      FP exp    = FP._1 / (FP._1 + x + _0_48 * x * x + _0_235 * x * x * x);

      FPVector3 change = current - target;
      FPVector3 temp   = (currentVelocity + omega * change) * deltaTime;
      currentVelocity = (currentVelocity - omega * temp) * exp;
      return target + (change + temp) * exp;
    }

    public static FPVector3 SmoothDampUnclamped(FPVector3 current, FPVector3 target, ref FPVector3 currentVelocity,
      FP smoothTime, FP maxSpeed, FP deltaTime) {
      FP _0_48  = (FP)48 / 100;
      FP _0_235 = (FP)235 / 1000;
      FP omega  = FP._2 / smoothTime;
      FP x      = omega * deltaTime;
      FP exp    = FP._1 / (FP._1 + x + _0_48 * x * x + _0_235 * x * x * x);

      FPVector3 change = target - current;
      // Clamp maximum speed
      FP maxChange = maxSpeed * smoothTime;
      change = FPVector3.ClampMagnitude(change, maxChange);
      FPVector3 temp = (currentVelocity - omega * change) * deltaTime;
      currentVelocity = (currentVelocity - omega * temp) * exp;
      return current + change + (temp - change) * exp;
    }
  }
}