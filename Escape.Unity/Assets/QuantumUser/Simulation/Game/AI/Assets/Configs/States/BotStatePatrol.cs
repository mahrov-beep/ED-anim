namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  [Serializable]
  public class BotStatePatrol : AssetObject {
    [Header("Движение")]
    public bool PressSprintButton = true;

    [Header("Пауза после достижения waypoint")]
    public FP PauseMin = FP._1;
    public FP PauseMax = FP._3;
  }
}
