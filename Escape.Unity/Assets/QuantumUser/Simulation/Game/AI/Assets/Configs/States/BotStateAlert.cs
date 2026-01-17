namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  [Serializable]
  public class BotStateAlert : AssetObject {
    [Header("Время ожидания после поворота (сек)")]
    public FP TurnWaitTime = FP._1;

    [Header("Время преследования если враг потерян (сек)")]
    public FP ChaseTime = FP._5;

    [Header("Радиус вокруг врага для выбора точки движения (м)")]
    public FP ApproachDistance = FP._3;
  }
}
