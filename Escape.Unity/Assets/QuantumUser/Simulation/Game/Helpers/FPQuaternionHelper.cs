namespace Quantum {
  using Photon.Deterministic;

  public static class FPQuaternionHelper {
    //
    // Yaw:  вращение вокруг оси FPVector3.Up
    // Pitch: вращение вокруг оси FPVector3.Right
    // Roll:  вращение вокруг оси FPVector3.Forward

    // yaw pitch roll должны быть в радианах
    public static FPQuaternion CreateFromYawPitchRoll(FPYawPitchRoll yawPitchRoll) {
      return FPQuaternion.CreateFromYawPitchRoll(yawPitchRoll.Yaw, yawPitchRoll.Pitch, yawPitchRoll.Roll);
    }

    // возвращает yaw pitch roll в радианах
    //
    // операция обратная CreateFromYawPitchRoll,
    // но может вернуть не совсем такие же углы,
    // а с небольшой погрешностью
    public static FPYawPitchRoll AsYawPitchRoll(FPQuaternion r) {
      // параметр -> что хранится
      // x -> z (чтобы получить значение Z нужно читать из X)
      // y -> x (чтобы получить значение X нужно читать из Y)
      // z -> y (чтобы получить значение Y нужно читать из Z)
      // итого нужен YXZ
      return FPYawPitchRoll.Create(r.AsEuler.YXZ * FP.Deg2Rad);
    }

    // возвращает yaw pitch roll в радианах
    public static FPYawPitchRoll LookRotationAsYawPitchRoll(FPVector3 forward, FPVector3 up) {
      return AsYawPitchRoll(FPQuaternion.LookRotation(forward, up));
    }
  }
}