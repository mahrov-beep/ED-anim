// ReSharper disable InconsistentNaming

namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;
  using UnityEngine;

  [Serializable]
  public class GameModeAsset : AssetObject {
    [Header("Если true, влючи в лунарке ShowDevScenes чтобы она отобразилась на карте")]
    public bool isDevelopOnly;

    public AssetRef<SimulationConfig> simulationConfig;
    public AssetRef<SystemsConfig>    systemsConfig;

    public Sprite MapSprite;

    public string gameModeKey => base.name;

    [Obsolete("Do not use name property. Use gameModeKey instead", true)]
    public new string name => base.name;

    public GameRules rule;

    [RequiredRef] public AssetRef<Map> map;

    [PropertyRange("@1", "@Quantum.Input.MAX_COUNT")]
    [LabelText("Max Players [?]")]
    [Tooltip("Define max number of players in qtn (default is 6, absolute max is 64)\n\n#pragma max_players 16")]
    public int maxPlayers;

    [Header("Какая стадия игры сколько длится")]
    public GameStateSettings GameStateSettings;

    [RequiredRef] public AssetRef<EntityPrototype> ItemBoxPrototype;
    [RequiredRef] public AssetRef<EntityPrototype> BackpackPrototype;

    [RequiredRef] public AssetRef<BotGlobalConfig> BotGlobalConfig;

    public FP OptimizeBotsRange = FP._100 + FP._10 * 5;

    public bool fillRoomWithBots = true;
    public bool fillRoomWithNpc  = true;

    [Header("задержка перед созданием челоботов (ботов-игроков)")]
    public FP feelRoomWithBotDelay = FP._3;

    [RequiredRef] public AssetRef<AimingAsset> aiming;


    [Header("Knockdown Settings")]
    public KnockSettingsConfig knockSettingsConfig;

    [SerializeField, HideInInspector]
    KnockSettings knockSettings = KnockSettings.Default;

    public KnockSettings GetKnockSettings() {
      if (knockSettingsConfig != null) {
        return knockSettingsConfig.GetKnockSettings();
      }

      return knockSettings.WithFallbacks();
    }

    // [Header("Резисты не смогут уменьшить шанс крита меньше этого %")]
    // public FP minCritChancePersent = FP._5;

    // [Header("Резисты не смогут уменьшить урон крита меньше этого %")]
    // public FP minCritDamagePersent = FP._5;
  }

  [Serializable]
  public struct GameStateSettings {
    public FP PresentationDurationSec;
    public FP GameDurationSec;
    public FP BeforeExitDurationSec;
  }

  [Serializable]
  public struct KnockSettings {
    public bool enabled;
    public FP knockDurationSec;
    public FP reviveDurationSec;
    public FP reviveDistance;
    public FP crawlSpeedMultiplier;
    public FP revivedHealthValue;
    public FP knockDamageImmunityDurationSec;
    public FP knockStartHealth;
    public FP knockHeightRatio;

    public static KnockSettings Default => new KnockSettings {
      enabled              = true,
      knockDurationSec     = FP._10 + FP._5,
      reviveDurationSec    = FP._4,
      reviveDistance       = FP._1 + FP._0_50,
      crawlSpeedMultiplier = FP._0_25 + FP._0_10,
      revivedHealthValue   = FP._10,
      knockDamageImmunityDurationSec = FP._0_50,
      knockStartHealth     = FP._10,
      knockHeightRatio     = FP._0_50,
    };

    public KnockSettings WithFallbacks() {
      var defaults = Default;
      bool hasCustomValues = knockDurationSec > FP._0 ||
                             reviveDurationSec > FP._0 ||
                             reviveDistance > FP._0 ||
                             crawlSpeedMultiplier > FP._0 ||
                             revivedHealthValue > FP._0 ||
                             knockDamageImmunityDurationSec > FP._0 ||
                             knockStartHealth > FP._0 ||
                             knockHeightRatio > FP._0;

      return new KnockSettings {
        enabled              = hasCustomValues ? true : defaults.enabled,
        knockDurationSec     = knockDurationSec > FP._0 ? knockDurationSec : defaults.knockDurationSec,
        reviveDurationSec    = reviveDurationSec > FP._0 ? reviveDurationSec : defaults.reviveDurationSec,
        reviveDistance       = reviveDistance > FP._0 ? reviveDistance : defaults.reviveDistance,
        crawlSpeedMultiplier = Clamp01(crawlSpeedMultiplier, defaults.crawlSpeedMultiplier),
        revivedHealthValue = revivedHealthValue > FP._0 ? revivedHealthValue : defaults.revivedHealthValue,
        knockDamageImmunityDurationSec = knockDamageImmunityDurationSec > FP._0
                ? knockDamageImmunityDurationSec
                : defaults.knockDamageImmunityDurationSec,
        knockStartHealth = knockStartHealth > FP._0 ? knockStartHealth : defaults.knockStartHealth,
        knockHeightRatio = knockHeightRatio > FP._0 ? FPMath.Clamp(knockHeightRatio, FP._0_10, FP._1) : defaults.knockHeightRatio,
      };
    }

    static FP Clamp01(FP value, FP fallback) {
      if (value <= FP._0) {
        return fallback <= FP._0 ? FP._0 : fallback;
      }
      return value >= FP._1 ? FP._1 : value;
    }
  }
}