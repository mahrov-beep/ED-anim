namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;
  using UnityEngine;

  [Serializable]
  public class UnitAsset : AssetObject {
    public FP baseMoveSpeed;
    public FP baseRotationSpeed;
    public FP baseMaxWeight     = 40;
    public FP baseLoadoutWidth  = 5;
    public FP baseLoadoutHeight = 10;
    public FP baseJumpImpulse   = FP._4;

    [ShowInInspector, ToggleGroup(nameof(IsSprintEnabled), groupTitle: "Sprint Settings")]
    public bool IsSprintEnabled {
      get => sprintSettings.enabled;
      private set => sprintSettings.enabled = value;
    }

    [ToggleGroup(nameof(IsSprintEnabled)), InlineProperty, HideLabel]
    public SprintSettings sprintSettings = SprintSettings.Default;

    [Header("Knife Settings")]
    public KnifeSettings knifeSettings = KnifeSettings.Default;
    public KnifeSettings GetKnifeSettings() => knifeSettings.WithFallbacks();

    [Header("Crouch Settings")]
    public CrouchSettings crouchSettings = CrouchSettings.Default;
    public CrouchSettings GetCrouchSettings() => crouchSettings.WithFallbacks();

    [Header("Jump Settings")]
    public JumpSettings jumpSettings = JumpSettings.Default;
    public JumpSettings GetJumpSettings() => jumpSettings.WithFallbacks();

    [Header("Aim Spread Modifiers")]
    public AimSpreadSettings aimSpreadSettings = AimSpreadSettings.Default;
    public AimSpreadSettings GetAimSpreadSettings() => aimSpreadSettings.WithFallbacks();

    [Header("Aim Crosshair Settings")]
    public AimCrosshairSettings aimCrosshairSettings = AimCrosshairSettings.Default;
    public AimCrosshairSettings GetAimCrosshairSettings() => aimCrosshairSettings.WithFallbacks();
  }

  [Serializable]
  public struct SprintSettings {
    public bool enabled;

    public FP maxStamina;

    [Help("Сколько уходит за 1 сек. действия")]
    public FP drainRate;
    
    [Help("Сколько приходит за 1 сек")]
    public FP regenRate;
    
    [Help("Пауза после действия, прежде чем начнётся реген")]
    public FP regenDelay;

    [Help("[0, 1] - Порог % «сколько нужно, чтобы начать» (0.25 → 25 %)")]
    public FP minStartRatio;

    public FP sprintSpeedMult;

    public static SprintSettings Default => new SprintSettings {
      maxStamina      = 25,
      drainRate       = 3,
      regenRate       = 2 + FP._0_50,
      regenDelay      = FP._0_50,
      minStartRatio   = FP._0_10 + FP._0_05,
      sprintSpeedMult = FP._1 + FP._0_50 + FP._0_20,
    };
  }

  [Serializable]
  public struct KnifeSettings {
    public FP Distance;
    public FP Damage;
    public FP Duration;
    public FP AttackEvent;
    public FP AttackAngleDegrees;
    public AssetRef<WeaponBasicAttackData> AttackData;

    public static KnifeSettings Default => new KnifeSettings {
      Distance = FP._2,
      Damage   = FP._1,
      Duration = FP._1,
      AttackEvent = FP._0_50,
      AttackAngleDegrees = FP._100 + FP._10 + FP._10,
      AttackData = default,
    };

    public KnifeSettings WithFallbacks() {
      var defaults         = Default;
      var resolvedDistance = Distance > FP._0 ? Distance : defaults.Distance;
      var resolvedDamage   = Damage > FP._0 ? Damage : defaults.Damage;
      var resolvedDuration = Duration > FP._0 ? Duration : defaults.Duration;
      var resolvedEvent    = AttackEvent > FP._0 ? AttackEvent : defaults.AttackEvent;
      resolvedEvent        = FPMath.Min(resolvedDuration, resolvedEvent);
      var resolvedAngle    = AttackAngleDegrees > FP._0 ? AttackAngleDegrees : defaults.AttackAngleDegrees;
      var clampedAngle     = FPMath.Clamp(resolvedAngle, FP._0, FP._360);
      var resolvedAttackData = AttackData.IsValid ? AttackData : defaults.AttackData;

      return new KnifeSettings {
        Distance   = resolvedDistance,
        Damage     = resolvedDamage,
        Duration   = resolvedDuration,
        AttackEvent = resolvedEvent,
        AttackAngleDegrees = clampedAngle,
        AttackData = resolvedAttackData,
      };
    }
  }

  [Serializable]
  public struct CrouchSettings {
    public FP CrouchSpeedMultiplier;
    public FP CrouchHeightRatio;      

    public static CrouchSettings Default => new CrouchSettings {
      CrouchSpeedMultiplier = FP._0_50,
      CrouchHeightRatio = FP._0_50,
    };

    public CrouchSettings WithFallbacks() {
      var defaults = Default;
      bool hasCustomSpeed = CrouchSpeedMultiplier > FP._0;
      bool hasCustomHeight = CrouchHeightRatio > FP._0;

      return new CrouchSettings {
        CrouchSpeedMultiplier = hasCustomSpeed ? CrouchSpeedMultiplier : defaults.CrouchSpeedMultiplier,
        CrouchHeightRatio = hasCustomHeight ? CrouchHeightRatio : defaults.CrouchHeightRatio,
      };
    }
  }

  [Serializable]
  public struct JumpSettings {
    [RangeEx(0, 1)]
    public FP StaminaCostPercent;

    [RangeEx(0, 1)]
    public FP MinStaminaPercentToJump;

    public static JumpSettings Default => new JumpSettings {
      StaminaCostPercent       = FP._0_20,
      MinStaminaPercentToJump  = FP._0_20,
    };

    public JumpSettings WithFallbacks() {
      var defaults = Default;

      return new JumpSettings {
        StaminaCostPercent      = Clamp01OrFallback(StaminaCostPercent, defaults.StaminaCostPercent),
        MinStaminaPercentToJump = Clamp01OrFallback(MinStaminaPercentToJump, defaults.MinStaminaPercentToJump),
      };
    }

    static FP Clamp01OrFallback(FP value, FP fallback) {
      if (value <= FP._0) {
        return fallback <= FP._0 ? FP._0 : fallback;
      }

      if (value >= FP._1) {
        return FP._1;
      }

      return value;
    }

    static FP PercentToAbsolute(FP percent, FP max) {
      if (max <= FP._0) {
        return FP._0;
      }

      percent = FPMath.Clamp(percent, FP._0, FP._1);
      return percent * max;
    }

    public FP GetRequiredStamina(FP max) {
      return PercentToAbsolute(MinStaminaPercentToJump, max);
    }

    public FP GetStaminaCost(FP max) {
      return PercentToAbsolute(StaminaCostPercent, max);
    }
  }

  [Serializable]
  public struct AimSpreadSettings {
    [Tooltip("Коэффициент разброса при прыжке (> 1 увеличивает разброс).")]
    public FP JumpSpreadMultiplier;

    [Tooltip("Коэффициент разброса при приседе (< 1 уменьшает разброс).")]
    public FP CrouchSpreadMultiplier;

    public static AimSpreadSettings Default => new AimSpreadSettings {
      JumpSpreadMultiplier   = FP._1_25,
      CrouchSpreadMultiplier = FP._0_50,
    };

    public AimSpreadSettings WithFallbacks() {
      var defaults = Default;

      return new AimSpreadSettings {
        JumpSpreadMultiplier   = EnsurePositive(JumpSpreadMultiplier, defaults.JumpSpreadMultiplier),
        CrouchSpreadMultiplier = EnsurePositive(CrouchSpreadMultiplier, defaults.CrouchSpreadMultiplier),
      };
    }

    static FP EnsurePositive(FP value, FP fallback) {
      if (value <= FP._0) {
        return fallback > FP._0 ? fallback : FP._1;
      }
      return value;
    }

    public FP ResolveMultiplier(bool isJumping, bool isCrouching) {
      if (isJumping) {
        return EnsurePositive(JumpSpreadMultiplier, FP._1_25);
      }

      if (isCrouching) {
        return EnsurePositive(CrouchSpreadMultiplier, FP._0_50);
      }

      return FP._1;
    }
  }

  [Serializable]
  public struct AimCrosshairSettings {
    [RangeEx(0, 1)]
    [Tooltip("Множитель AimPercent при прыжке (меньше 1 — прицел шире).")]
    public FP JumpAimPercentScale;

    [Tooltip("Множитель AimPercent при приседе (больше 1 — прицел уже).")]
    public FP CrouchAimPercentScale;

    [Tooltip("Скорость сглаживания изменения прицела.")]
    public FP TransitionSpeed;

    public static AimCrosshairSettings Default => new AimCrosshairSettings {
      JumpAimPercentScale   = FP._0_75,
      CrouchAimPercentScale = FP._1_20,
      TransitionSpeed       = FP._6,
    };

    public AimCrosshairSettings WithFallbacks() {
      var defaults = Default;

      return new AimCrosshairSettings {
        JumpAimPercentScale   = Clamp01OrFallback(JumpAimPercentScale, defaults.JumpAimPercentScale),
        CrouchAimPercentScale = EnsurePositive(CrouchAimPercentScale, defaults.CrouchAimPercentScale),
        TransitionSpeed       = EnsurePositive(TransitionSpeed, defaults.TransitionSpeed),
      };
    }

    static FP Clamp01OrFallback(FP value, FP fallback) {
      if (value <= FP._0) {
        return fallback <= FP._0 ? FP._0 : fallback;
      }

      if (value >= FP._1) {
        return FP._1;
      }

      return value;
    }

    static FP EnsurePositive(FP value, FP fallback) {
      if (value <= FP._0) {
        return fallback > FP._0 ? fallback : FP._1;
      }

      return value;
    }

    public FP ResolveAimPercent(FP basePercent, bool isJumping, bool isCrouching) {
      var percent = basePercent;

      if (isJumping) {
        percent *= EnsurePositive(JumpAimPercentScale, Default.JumpAimPercentScale);
      }
      else if (isCrouching) {
        percent *= EnsurePositive(CrouchAimPercentScale, Default.CrouchAimPercentScale);
      }

      return FPMath.Clamp(percent, FP._0, FP._1);
    }
  }
}
