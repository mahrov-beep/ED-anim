namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;
  public class ReduceDamageByDistanceConfig : AssetObject {
    [Header("Минимальная дистанция для начала уменьшения")]
    public FP distanceThreshold = FP._2;
    public FPAnimationCurve curve;

    public unsafe void ReduceDamageByDistance(Attack* attack, FP distance) {
      if (distanceThreshold > distance) {
        return;
      }

      ref FP damage = ref attack->HealthApplicator.Value;

      FP coefficient = curve.Evaluate(distance);

      // чем выше DistanceDamageMultiplier, тем больше урона
      // если DistanceDamageMultiplier=100%
      // то урон максимальный даже в конечной точке
      FP t = FPMath.InverseLerp(FP._1, FP._2,
              FP._1 * attack->DistanceDamageMultiplier);

      coefficient.LerpTo(FP._1, t);
      
      damage *= coefficient;
    }
  }
}