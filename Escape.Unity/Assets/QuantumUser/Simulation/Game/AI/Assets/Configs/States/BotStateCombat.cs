namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  [Serializable]
  public class BotStateCombat : AssetObject {
    [Header("Время прицеливания перед стрельбой (сек)")]
    public FP AimDelaySeconds = FP._0_50;

    [Header("Радиус выбора точки вокруг цели для перебежки между очередями")]
    public FP RepositionRadius = FP._3;

    [Header("Величина отклонения стика при перебежке (0-1)")]
    [Range(0f, 1f)]
    public FP RepositionMagnitude = FP._0_20;
  }
}
