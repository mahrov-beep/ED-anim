namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  [Serializable]
  public class BotMemoryModule : AssetObject {
    [Header("Скорость угасания памяти о цели (секунды) / Memory decay time (seconds)")]
    public FP DecayTime = 10;

    [Header("Время до забывания цели (секунды) / Time before target is forgotten (seconds)")]
    public FP ForgetTime = 30;

    [Header("Макс. целей в памяти / Max tracked targets")]
    public int MaxTrackedTargets = 10;
  }
}
