namespace Quantum {
  using System.Runtime.CompilerServices;
  using Photon.Deterministic;

  public partial struct FPYawPitchRoll {
    public FPVector3 AsFPVector3 {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        FPVector3 r;
        r.X = Yaw;
        r.Y = Pitch;
        r.Z = Roll;
        return r;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPYawPitchRoll CreateYaw(FP yaw) {
      FPYawPitchRoll r;
      r.Yaw   = yaw;
      r.Pitch = FP._0;
      r.Roll  = FP._0;
      return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPYawPitchRoll Create(FP yaw, FP pitch, FP roll) {
      FPYawPitchRoll r;
      r.Yaw   = yaw;
      r.Pitch = pitch;
      r.Roll  = roll;
      return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPYawPitchRoll Create(FPVector3 yawPitchRoll) {
      FPYawPitchRoll r;
      r.Yaw   = yawPitchRoll.X;
      r.Pitch = yawPitchRoll.Y;
      r.Roll  = yawPitchRoll.Z;
      return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPYawPitchRoll operator +(FPYawPitchRoll a, FPYawPitchRoll b) {
      a.Yaw.RawValue   += b.Yaw.RawValue;
      a.Pitch.RawValue += b.Pitch.RawValue;
      a.Roll.RawValue  += b.Roll.RawValue;
      return a;
    }
  }
}