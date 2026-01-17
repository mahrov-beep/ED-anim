namespace Quantum {
  using Photon.Deterministic;

  public static class FPYawPitchRollHelper {
    public static FPYawPitchRoll SmoothDampUnclamped(FPYawPitchRoll current, FPYawPitchRoll target,
      ref FPVector3 currentVelocity, FP smoothTime, FP deltaTime) {
      return FPYawPitchRoll.Create(FPVector3Helper.SmoothDampUnclamped(current.AsFPVector3, target.AsFPVector3,
        ref currentVelocity, smoothTime, deltaTime));
    }

    public static FPYawPitchRoll SmoothDampUnclamped(FPYawPitchRoll current, FPYawPitchRoll target,
      ref FPVector3 currentVelocity, FP smoothTime, FP maxSpeed, FP deltaTime) {
      return FPYawPitchRoll.Create(FPVector3Helper.SmoothDampUnclamped(current.AsFPVector3, target.AsFPVector3,
        ref currentVelocity, smoothTime, maxSpeed, deltaTime));
    }
  }
}