namespace Quantum {
  using System;
  using System.Runtime.CompilerServices;
  using Photon.Deterministic;
  using Unity.IL2CPP.CompilerServices;

  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  public partial struct FPBoostedMultiplier {
    public static FPBoostedMultiplier One => new();

    //public FPBoostedMultiplier Inversed => new() { AdditiveMultiplierMinus1 = -AdditiveMultiplierMinus1, BoostCount = BoostCount };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPBoostedValue operator *(FP value, FPBoostedMultiplier multiplier) {
      return new FPBoostedValue {
        BaseValue                = value,
        AdditiveMultiplierMinus1 = multiplier.AdditiveMultiplierMinus1,
      };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPBoostedMultiplier operator +(FPBoostedMultiplier a, FPBoostedMultiplier b) {
      return new FPBoostedMultiplier {
        AdditiveMultiplierMinus1 = a.AdditiveMultiplierMinus1 + b.AdditiveMultiplierMinus1,
      };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPBoostedMultiplier operator *(FPBoostedMultiplier a, FPBoostedMultiplier b) {
      return new FPBoostedMultiplier {
        AdditiveMultiplierMinus1 = (FP._1 + a.AdditiveMultiplierMinus1) * (FP._1 + b.AdditiveMultiplierMinus1) - FP._1,
      };
    }
  }
}