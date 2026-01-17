namespace Quantum {
  using Photon.Deterministic;

  public static class FPMathHelper {
    public static FP SmoothDampUnclamped(FP current, FP target, ref FP currentVelocity, FP smoothTime, FP deltaTime) {
      FP _0_48  = (FP)48 / 100;
      FP _0_235 = (FP)235 / 1000;
      FP omega  = FP._2 / smoothTime;
      FP x      = omega * deltaTime;
      FP exp    = FP._1 / (FP._1 + x + _0_48 * x * x + _0_235 * x * x * x);
      FP change = current - target;
      FP temp   = (currentVelocity + omega * change) * deltaTime;
      currentVelocity = (currentVelocity - omega * temp) * exp;
      return target + (change + temp) * exp;
    }

    public static FP SmoothDampUnclamped(FP current, FP target, ref FP currentVelocity, FP smoothTime, FP maxSpeed, FP deltaTime) {
      FP _0_48  = (FP)48 / 100;
      FP _0_235 = (FP)235 / 1000;
      FP omega  = FP._2 / smoothTime;
      FP x      = omega * deltaTime;
      FP exp    = FP._1 / (FP._1 + x + _0_48 * x * x + _0_235 * x * x * x);
      FP change = target - current;
      // Clamp maximum speed
      FP maxChange = maxSpeed * smoothTime;
      change = FPMath.Clamp(change, -maxChange, maxChange);
      FP temp = (currentVelocity - omega * change) * deltaTime;
      currentVelocity = (currentVelocity - omega * temp) * exp;
      return current + change + (temp - change) * exp;
    }

    // source: https://github.com/LSBUGPG/SmoothDamp
    public static FP SmoothDampMovingTarget(FP current, FP target, ref FP currentVelocity, FP previousTarget,
      FP smoothTime, FP maxSpeed, FP deltaTime) {
      FP output;
      if (target == current || (previousTarget < current && current < target) || (previousTarget > current && current > target)) {
        // currently on target or target is passing through
        output          = current;
        currentVelocity = 0;
      }
      else {
        // apply original smoothing
        output = SmoothDampUnclamped(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        if ((target > current && output > target) || (target < current && output < target)) {
          // we have overshot the target
          output          = target;
          currentVelocity = 0;
        }
      }

      return output;
    }
    
    public static short RoundToInt16(FP value)
    {
      return (short)((value.RawValue & ushort.MaxValue) >= FP._0_50.RawValue
              ? (value.RawValue >> 16) + 1L
              : value.RawValue >> 16);
    }
  }
}