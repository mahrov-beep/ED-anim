namespace Quantum {
  using Photon.Deterministic;

  public partial struct FPBoostedValue {
    public FP    AsFP    => (BaseValue + AdditiveValue) * (FP._1 + AdditiveMultiplierMinus1);
    public float AsFloat => AsFP.AsFloat;

    public static implicit operator FP(FPBoostedValue boostedValue) {
      return boostedValue.AsFP;
    }

    public static implicit operator FPBoostedValue(FP baseValue) {
      return new FPBoostedValue { BaseValue = baseValue };
    }

    public override string ToString() {
      return AsFP.ToString();
    }
  }
}