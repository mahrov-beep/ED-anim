using Photon.Deterministic;

namespace Quantum {
  using UnityEngine;
  public static class LerpExtension {
    public static void LerpTo(this ref FPVector2 current, FPVector2 target, FP t) {
      current = FPVector2.Lerp(current, target, t);
    }

    public static void LerpTo(this ref FPVector3 current, FPVector3 target, FP t) {
      current = FPVector3.Lerp(current, target, t);
    }

    public static void LerpTo(this ref FP current, FP target, FP t) {
      current = FPMath.Lerp(current, target, t);
    }

    public static void LerpTo(this ref float current, float target, float t) {
      current = Mathf.Lerp(current, target, t);
    }

    public static void PositionLerpTo(this Transform transform, Vector3 target, float t) {
      var pos = transform.position;

      pos = Vector3.Lerp(pos, target, t);

      transform.position = pos;
    }
  }
}