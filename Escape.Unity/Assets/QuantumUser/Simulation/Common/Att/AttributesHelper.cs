namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;
  using Collections;
  using Photon.Deterministic;
  using Unity.IL2CPP.CompilerServices;
  using static EModifierAppliance;
  using static EModifierOperation;

  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public static unsafe class AttributesHelper {
    static readonly FP DefaultValue = FP._0;
    public static void SetForceVisibleInInvisibilityZone(Frame f, EntityRef e, FP durationSec) {
      if (durationSec <= FP._0) {
        return;
      }

      ChangeAttribute(f, e, EAttributeType.Set_ForceVisibleInInvisibilityZone,
              Temporary, Add, FP._1, durationSec);
    }

    public static void SetForceVisibleOnMap(Frame f, EntityRef e, FP durationSec) {
      EAttributeType.Set_ForceVisibleOnMap.ChangeAttribute(f, e,
              Temporary, Add, FP._1, durationSec);
    }

    public static void SetImmunity(Frame f, EntityRef e, FP durationSec) {
      if (durationSec <= FP._0) {
        return;
      }

      ChangeAttribute(f, e, EAttributeType.Set_Immunity,
              Temporary, Add, FP._1, durationSec);
    }

    public static void SetMovementLock(Frame f, EntityRef e, FP durationSec) {
      if (durationSec <= FP._0) {
        return;
      }

      ChangeAttribute(f, e, EAttributeType.Set_LockMovement,
              Temporary, Add, FP._1, durationSec);
    }

    public static void SetRotationLock(Frame f, EntityRef e, FP durationSec) {
      if (durationSec <= FP._0) {
        return;
      }

      ChangeAttribute(f, e, EAttributeType.Set_LockRotation,
              Temporary, Add, FP._1, durationSec);
    }

    public static void ChangeAttribute(this EAttributeType type,
            Frame f,
            EntityRef e,
            EModifierAppliance appliance,
            EModifierOperation operation,
            FP value,
            FP duration,
            EAttributeType valueMultiplier = EAttributeType.None) {

      ChangeAttribute(f, e, type, appliance, operation, value, duration, valueMultiplier);
    }

    public static void ChangeAttribute(Frame f, EntityRef e,
            EAttributeType type,
            EModifierAppliance appliance,
            EModifierOperation operation,
            FP value,
            FP duration,
            EAttributeType valueMultiplier = EAttributeType.None) {

      var att    = f.GetPointer<Attributes>(e);
      var attMap = f.ResolveDictionary(att->DataDictionary);

      bool hasAttData = attMap.TryGetValuePointer(type,
              out AttributeData* attData);

      if (!hasAttData) {
        attMap.TryAdd(type, new AttributeData { InitialValue = DefaultValue, });
        attMap.TryGetValuePointer(type, out attData);

        attData->Init(f, e);
      }

      AttributeModifier modifier = new AttributeModifier {
              Amount            = value,
              AmountMultiplier  = valueMultiplier,
              ModifierAppliance = appliance,
              ModifierOperation = operation,
              Duration          = duration,
      };

      attData->AddModifier(f, e, modifier);

      if (appliance != OneTime) {
        f.Add<AttributesTickable>(e);
      }
    }

    public static FP GetCurrentValue(Frame f, EntityRef e, EAttributeType type) {
      return TryGetCurrentValue(f, e, type, out var currentValue) ? currentValue : DefaultValue;
    }

    public static bool TryGetCurrentValue(Frame f, EntityRef e, EAttributeType type, out FP currentValue) {
      var att = f.GetPointer<Attributes>(e);
      return TryGetCurrentValue(f, att->DataDictionary, type, out currentValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetCurrentValue(Frame f, QDictionaryPtr<EAttributeType, AttributeData> data, EAttributeType type,
      out FP currentValue) {
      var attMap = f.ResolveDictionary(data);

      if (!attMap.TryGetValuePointer(type, out AttributeData* attData)) {
        currentValue = DefaultValue;
        return false;
      }

      currentValue = attData->CurrentValue;
      return true;
    }

    public static void ApplyAsAdditiveValueOn(ref FPBoostedValue value, Frame f, EntityRef e, EAttributeType type) {
      var att = f.GetPointer<Attributes>(e);
      ApplyAsAdditiveValueOn(ref value, f, att->DataDictionary, type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyAsAdditiveValueOn(ref FPBoostedValue value, Frame f, 
      QDictionaryPtr<EAttributeType, AttributeData> data, EAttributeType type) {
      EnumExt<EAttributeType>.ValidateNameStartsWith(type, "AdditiveBoost_");

      if (!TryGetCurrentValue(f, data, type, out var currentValue)) {
        return;
      }

      value.AdditiveValue += currentValue;
    }

    /// <summary>
    /// DO NOT USE INTO SIMULATION
    /// </summary>
    public static void UNSAFE_ApplyAsPercentMultiplierOn(ref float value, Frame f, EntityRef e, EAttributeType type,
            EModifierOperation operation = Add) {

      EnumExt<EAttributeType>.ValidateNameStartsWith(type, "PercentBoost_");

      var additionalValue = value * 0.01f * GetCurrentValue(f, e, type).AsFloat;
      switch (operation) {
        case Add:      value += additionalValue; break;
        case Subtract: value -= additionalValue; break;
      }
    }
    
    public static void ApplyAsPercentMultiplierOn(ref FPBoostedMultiplier value, Frame f, EntityRef e, EAttributeType type) {
      var att = f.GetPointer<Attributes>(e);
      ApplyAsPercentMultiplierOn(ref value, f, att->DataDictionary, type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyAsPercentMultiplierOn(ref FPBoostedMultiplier value, Frame f, 
      QDictionaryPtr<EAttributeType, AttributeData> data, EAttributeType type) {
      EnumExt<EAttributeType>.ValidateNameStartsWith(type, "PercentBoost_");

      if (!TryGetCurrentValue(f, data, type, out var currentValue)) {
        return;
      }

      value.AdditiveMultiplierMinus1 += FP._0_01 * currentValue;
    }

    public static void ApplyAsPercentMultiplierOn(ref FP value, Frame f, EntityRef e, EAttributeType type,
            EModifierOperation operation = Add) {

      EnumExt<EAttributeType>.ValidateNameStartsWith(type, "PercentBoost_");

      var additionalValue = value * FP._0_01 * GetCurrentValue(f, e, type);
      switch (operation) {
        case Add:      value += additionalValue; break;
        case Subtract: value -= additionalValue; break;
      }
    }

    public static void ApplyAsPercentMultiplierOn(ref int value, Frame f, EntityRef e, EAttributeType type,
            EModifierOperation operation = Add) {

      EnumExt<EAttributeType>.ValidateNameStartsWith(type, "PercentBoost_");

      var attributeValue  = GetCurrentValue(f, e, type);
      var additionalValue = FPMath.RoundToInt(value * FP._0_01 * attributeValue);
      switch (operation) {
        case Add:      value += additionalValue; break;
        case Subtract: value -= additionalValue; break;
      }
    }

    public static void ApplyAsPercentMultiplierOn(ref short value, Frame f, EntityRef e, EAttributeType type,
            EModifierOperation operation = Add) {

      EnumExt<EAttributeType>.ValidateNameStartsWith(type, "PercentBoost_");

      var attributeValue  = GetCurrentValue(f, e, type);
      var additionalValue = FPMathHelper.RoundToInt16(value * FP._0_01 * attributeValue);
      switch (operation) {
        case Add:      value += additionalValue; break;
        case Subtract: value -= additionalValue; break;
      }
    }

    /// <summary>
    /// DO NOT USE INTO SIMULATION
    /// </summary>
    public static void UNSAFE_ApplyPercentMultiplierOn(this EAttributeType type, ref float value, Frame f, EntityRef e,
            EModifierOperation operation = Add) {
      UNSAFE_ApplyAsPercentMultiplierOn(ref value, f, e, type, operation);
    }

    [Obsolete("Use (Unit/Weapon).CurrentStats instead", true)]
    public static void ApplyPercentMultiplierOn(this EAttributeType type, ref FP value, Frame f, EntityRef e,
            EModifierOperation operation = Add) {
      ApplyAsPercentMultiplierOn(ref value, f, e, type, operation);
    }

    [Obsolete("Use (Unit/Weapon).CurrentStats instead", true)]
    public static void ApplyPercentMultiplierOn(this EAttributeType type, ref int value, Frame f, EntityRef e,
            EModifierOperation operation = Add) {
      ApplyAsPercentMultiplierOn(ref value, f, e, type, operation);
    }

    [Obsolete("Use (Unit/Weapon).CurrentStats instead", true)]
    public static void ApplyPercentMultiplierOn(this EAttributeType type, ref short value, Frame f, EntityRef e,
            EModifierOperation operation = Add) {
      ApplyAsPercentMultiplierOn(ref value, f, e, type, operation);
    }

    public static bool IsValueSet(Frame f, EntityRef e, EAttributeType type) {
      EnumExt<EAttributeType>.ValidateNameStartsWith(type, "Set_");

      return GetCurrentValue(f, e, type) > FP._0_50;
    }

    public static bool IsValueSet(this EAttributeType type, Frame f, EntityRef e) {
      EnumExt<EAttributeType>.ValidateNameStartsWith(type, "Set_");

      return GetCurrentValue(f, e, type) > FP._0_50;
    }
  }
}