namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;
  using UnityEngine.Serialization;

  [Serializable]
  public class BotVisionModule : AssetObject {
    [FormerlySerializedAs("UpdateInterval")]
    [Header("Интервал обновления (секунды) / Update interval (seconds)")]
    public FP updateInterval = FP._0_20;

    [FormerlySerializedAs("ReactionDelay")]
    [Header("Задержка реакции (секунды) / Reaction delay (seconds)")]
    public FP reactionDelay = FP._0_50;

    [FormerlySerializedAs("Radius")]
    [Header("Радиус зрения (метры) / Vision radius (meters)")]
    public FP forwardRadius = 30;

    [FormerlySerializedAs("Angle")]
    [Header("Угол обзора (градусы) / Field of view angle (degrees)")]
    public FP forwardAngle = 90;

    [FormerlySerializedAs("BackRadius")]
    [Header("Радиус заднего зрения (метры) / Back vision radius (meters)")]
    public FP backRadius = 10;

    [FormerlySerializedAs("BackAngle")]
    [Header("Угол заднего обзора (градусы) / Back field of view angle (degrees)")]
    public FP backAngle = 180;

  }
}