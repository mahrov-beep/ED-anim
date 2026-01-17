namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;
  using UnityEngine;
  using UnityEngine.Serialization;

  [Serializable]
  public class BotGlobalConfig : AssetObject {
    [Serializable]
    public struct BTAgentConfig {
      public AssetRef<BTRoot>       Tree;
      public AssetRef<AIConfigBase> AIConfig;
    }

    [Serializable]
    public struct GizmoSettings {
      public bool     Enabled;
      public ColorRGBA Color;
    }

    [Serializable]
    public class DebugSettings {
      [FoldoutGroup("Vision Forward")]
      public bool     VisionForwardEnabled = true;
      [FoldoutGroup("Vision Forward")]
      public ColorRGBA VisionForwardColor = ColorRGBA.Green;

      [FoldoutGroup("Vision Back")]
      public bool     VisionBackEnabled = true;
      [FoldoutGroup("Vision Back")]
      public ColorRGBA VisionBackColor = ColorRGBA.Yellow;

      [FoldoutGroup("Hearing")]
      public bool     HearingEnabled = true;
      [FoldoutGroup("Hearing")]
      public ColorRGBA HearingColor = new ColorRGBA { R = 0, G = 191, B = 255, A = 255 };

      [FoldoutGroup("Attack Target")]
      public bool     AttackTargetEnabled = true;
      [FoldoutGroup("Attack Target")]
      public ColorRGBA AttackTargetColor = ColorRGBA.Red;

      [FoldoutGroup("Movement Target")]
      public bool     MovementTargetEnabled = true;
      [FoldoutGroup("Movement Target")]
      public ColorRGBA MovementTargetColor = ColorRGBA.Cyan;
    }

    public int BTUpdateTickInterval = 10;

    [FormerlySerializedAs("BTAgent")]
    [Header("Значения могут быть переопределены в " + nameof(BotSpawnPoint))]
    public BTAgentConfig btAgentConfig;

    public AssetRef<BotDifficultiesConfig> StatsMultipliers;

    public AssetRef<BotStatePatrol> PatrolState;
    public AssetRef<BotStateAlert> AlertState;
    public AssetRef<BotStateCombat> CombatState;

    [Header("Дефолтные модули восприятия")]
    public AssetRef<BotVisionModule> VisionModule;
    public AssetRef<BotHearingModule> HearingModule;
    public AssetRef<BotMemoryModule> MemoryModule;

    [Header("Это рандомные комплекты для челоботов")]
    public LoadoutConfig[] Loadouts;

    [TabGroup("Debug")]
    public DebugSettings Debug = new DebugSettings();
  }
}