namespace Game.ECS.Systems.Unit {
    using Photon.Deterministic;
    using Quantum;
    using UnityEngine;
    [CreateAssetMenu(menuName = "Create " + nameof(MinimapConfig), fileName = nameof(MinimapConfig), order = 0)]
    public class MinimapConfig : ScriptableObject {
        [Header("Задержка скрытия на карте после потери из поля зрения"), RangeEx(0.2f, 3)]
        public FP baseHideDuration = FP._0_50;
        
        [Header("Базовое значение задержки скрытия на карте после выстрела"), RangeEx(0, 3)]
        public FP baseShotVisiblyDuration = FP._1;
        
        [Header("Дальность обзора миникарты в юнитах, ДО применения Att")]
        public float baseVisiblyRadius = 35;

        [Header("Скорость изменения дальности обзора на миникарте")]
        public float visibleRadiusChangeSpeed = 30;
    }
}