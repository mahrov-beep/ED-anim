namespace Game.ECS.Systems.Grenade {
    using UnityEngine;

    [CreateAssetMenu(menuName = "Create " + nameof(GrenadeIndicatorConfig), fileName = nameof(GrenadeIndicatorConfig), order = 0)]
    public class GrenadeIndicatorConfig : ScriptableObject {
        [Header("Detection")]
        [Tooltip("Multiplier for explosion radius to determine danger zone")]
        public float dangerRadiusMultiplier = 2f;
        
        [Tooltip("Maximum distance to detect grenades")]
        public float maxDetectionRange = 50f;
        
        [Tooltip("Maximum number of grenades to show simultaneously")]
        public int maxVisibleIndicators = 3;
        
        [Tooltip("Show indicators for grenades outside the screen")]
        public bool showOffScreenIndicators = true;

        [Header("Performance")]
        [Tooltip("Update interval in seconds (0 = every frame)")]
        [Range(0f, 0.5f)]
        public float updateInterval = 0f;       
    }
}

