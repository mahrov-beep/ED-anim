namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;

  [Serializable]
  public class BotDifficultiesConfig : AssetObject {

    [TabGroup("tabs", "Multipliers")]
    public FP Damage = FP._1;

    [TabGroup("tabs", "Multipliers")]
    public FP Spread = FP._1;

    [TabGroup("tabs", "Multipliers")]
    public FP MoveSpeed = FP._0_75;

    [TabGroup("tabs", "Burst Fire Delays")]
    [InfoBox(
      "Controls BurstFireService. FireSeconds is the shooting phase length; RestSeconds is the pause length. " +
      "Service blocks shooting during Rest by holding SecondaryAction. " +
      "Configure per difficulty and assign via SpawnPoint overrides.")]
    public FP BurstFireSeconds = FP._2;

    [TabGroup("tabs", "Burst Fire Delays")]
    public FP BurstRestSeconds = FP._1_50;
  }
}