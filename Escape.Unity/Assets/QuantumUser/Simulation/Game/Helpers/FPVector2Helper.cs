using Photon.Deterministic;

namespace Quantum {
  public static unsafe class FPVector2Helper {
    public static FPVector2 RandomInsideCircle(RNGSession* RNG, FPVector2 origin, FP radius) {
      var angle     = RNG->NextInclusive(FP._0, FP.PiTimes2);
      var direction = FPVector2.Rotate(FPVector2.Up, angle);
      var distance  = RNG->NextInclusive(FP._0, radius);

      return origin + direction * distance;
    }

    public static FPVector2 Project(FPVector2 vector, FPVector2 onNormal) {
      FP num1 = FPVector2.Dot(onNormal, onNormal);
      FP num2 = FPVector2.Dot(vector, onNormal);
      return new FPVector2(onNormal.X * num2 / num1, onNormal.Y * num2 / num1);
    }
  }
}