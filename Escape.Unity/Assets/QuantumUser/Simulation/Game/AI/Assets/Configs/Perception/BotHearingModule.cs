namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  [Serializable]
  public class BotHearingModule : AssetObject {
    [Header("Радиус слуха (метры) / Hearing radius (meters)")]
    public FP Radius = 50;

    [Header("Порог скорости для обнаружения (м/с) / Speed threshold to be heard (m/s)")]
    public FP SpeedThreshold = FP._2;

    [Header("Интервал обновления (секунды) / Update interval (seconds)")]
    public FP UpdateInterval = FP._0_10;

    [Space]
    [Header("Игнорировать других NPC / Ignore other NPCs")]
    public bool IgnoreOtherNpc = true;

    [Space]
    [Header("Невидимые боты игнорируют друг друга / Invisible bots ignore each other\nTrue рекомендуется для производительности / True recommended for performance")]
    public bool InvisibleBotsIgnoreEachOther = true;
  }
}
