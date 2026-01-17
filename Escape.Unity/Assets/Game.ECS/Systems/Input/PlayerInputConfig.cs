namespace Game.ECS.Systems.Input {
    using Photon.Deterministic;
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Serialization;

    [CreateAssetMenu(menuName = "Create " + nameof(PlayerInputConfig), fileName = nameof(PlayerInputConfig), order = 0)]
    public class PlayerInputConfig : ScriptableObject {
        [TabGroup("InputPoller")][Header("Cкорость движения в зависимости от отклонения стика")]
        public FPAnimationCurve movementSpeedCurve;

        [TabGroup("InputPoller")][Header("[Осторожно] Множитель вектора свайпа по экрану из Input.actions[\"LookDelta\"]")]
        [Tooltip("По сути это чувствительность свайпа, как скорость мыши в винде")]
        [RangeEx(0.01, 0.10)]
        public FP lookDeltaMultiplier = FP._0_01;
        
        [TabGroup("InputPoller")]
        [Header("Дополнительный множитель для дельты")]
        public FP lookDeltaMultiplierImAim = FP._0_33;

        [TabGroup("AimAssist")][Header("Зоны Aim-Assist (коэффициенты от размера врага на экране)")]
        [Header("Коэффициент ширины внешней зоны (домножается на ширину/высоту врага в пикселях)")]
        public float outerZoneMultiplierWidth = 7f;

        [TabGroup("AimAssist")]
        [Header("Коэффициент высоты внешней зоны (домножается на ширину/высоту врага в пикселях)")]
        public float outerZoneMultiplierHeight = 2.6f;

        [TabGroup("AimAssist")]
        [Header("Коэффициент ширины внутренней зоны (домножается на ширину/высоту врага в пикселях)")]
        public float innerZoneMultiplierWidth = 1.5f;

        [TabGroup("AimAssist")]
        [Header("Коэффициент высоты внутренней зоны (домножается на ширину/высоту врага в пикселях)")]
        public float innerZoneMultiplierHeight = 1.2f;

        [TabGroup("AimAssist")]
        [Header("Время(сек) сколько надо удерживать прицел во внешней зоне для захвата цели")]
        public float timeToHoldAimInOuterRectForSelectTarget = 1f;

        [TabGroup("AimAssist")]
        [Header("Время(сек) для потери цели, если прицел не во внешней зоне это время")]
        public float timeWithoutCrossOuterZoneRectForLostTarget = 2f;

        [TabGroup("AimAssist")]
        [Header("Время(сек) которое надо держать прицел на персонаже для смены цели ассиста")]
        public float timeWithoutAimRaycastForLostTarget = 1.2f;

        [TabGroup("AimAssist")]
        [Header("Aim-Assist Distances")]
        public float aimAssistMaxDistance = 40f;

        [TabGroup("AimAssist")]
        [RangeEx(0, 10f)]
        [Header("AssistPower увеличивается со скоростью P, если внутри зоны прицел двигается к противнику;")]
        public float p = 0.2f;

        [TabGroup("AimAssist")]
        [RangeEx(0, 10f)]
        [Header("(ДОЛЖНО БЫТЬ В +- 4 раза больше P) AssistPower уменьшается со скоростью M, если движение идет в сторону от противника")]
        public float m = 0.8f;

        [TabGroup("AimAssist")]
        [Header("Скорость уменьшения AssistPower если прицел не во внутренней зоне")]
        public float aimAssistPowerDecreaseOutInnerZone = 0.03f;

        [TabGroup("AimAssist")]
        [RangeEx(0, 10f)]
        [Header("Скорость увеличения AssistPower если прицел во внутренней зоне")]
        public float aimAssistPowerIncreaseInInnerZone = 1f;

        [TabGroup("AimAssist")]
        [RangeEx(1, 10f)]
        [Header("[спроси если хочешь менять] AssistPower растет до этого значения, потом текущий AssistPower делится aimAssistPowerMax")]
        public float aimAssistPowerMax = 1;

        [TabGroup("AimAssist")]
        [RangeEx(0.01, 1f)]
        [Header("[трогать осторожно] (Deg) считаем что прицел не двигается, если дельта поворота ниже этого значения")]
        public FP horizontalThresholdDeg = FP._0_33;

        [TabGroup("AimAssist")]
        [RangeEx(0, 10f)]
        [Header("[трогать осторожно] DEG мертвая зона вокруг цели")]
        public FP deadZoneAngle = FP._3;

        [Space(20)]
        [TabGroup("AimAssist")]
        [RangeEx(1, 20)]
        [Header("DEG ограничение на доворот угла который отправится в симуляцию")]
        public FP maxAssistStepDeg;

        [TabGroup("AimAssist")]
        [RangeEx(1, 10)]
        [Header("Множитель силы доводки для НЕПОДВИЖНОГО прицела")]
        public FP assistPowerMult = FP._3;

        [TabGroup("AimAssist")]
        [RangeEx(0.1, 10)]
        [Header("Множитель силы доводки для ДВИЖУЩЕГОСЯ прицела")]
        public FP assistPowerMultForAimMove = FP._0_33;

        [TabGroup("PC Controls")]
        [Header("Чувствительность мыши (Standalone)")]
        [Tooltip("Дефолтное значение множителя чувствительности")]
        public float MouseSensitivityDefault = 1.0f;

        [TabGroup("PC Controls")]
        [Tooltip("Минимальное значение множителя чувствительности")]
        public float MouseSensitivityMin = 0.1f;

        [TabGroup("PC Controls")]
        [Tooltip("Максимальное значение множителя чувствительности")]
        public float MouseSensitivityMax = 5.0f;

        // по сути эти значения константны, это высота и ширина колайдера на чарактере, в ряд ли оно будет меняться
     //   [TabGroup("Лучше не трогать")]
     //   [Header("НЕ ТРОГАТЬ - Высота врага в юнитах (world units)")]
     //   public float characterHeight = 1.9f;

        [TabGroup("Лучше не трогать")]
        [Header("НЕ ТРОГАТЬ - Ширина врага в юнитах (world units)")]
        public float characterWidth = 0.5f;

        [BoxGroup("AimAssist - Debug")]
        public AimAssistZoneDrawer debugDrawPrefab;
    }

}