// ReSharper disable InconsistentNaming
namespace Game.ECS.Systems.Unit {
    using Photon.Deterministic;
    using UnityEngine;
    [CreateAssetMenu(menuName = "Create CueUIVisualizeConfig", fileName = "CueUIVisualizeConfig", order = 0)]
    public class CueUIVisualizeConfig : ScriptableObject {
        public float stepListenDistance  = 20;
        public FP    shootListenDistance = 20;
        public FP    realSpeedLimit      = FP._2;

        [Space]
        public float stepMarkerLifetime = 2f;
        public float shootMarkerLifetime = 2f;
        
        [Space]
        public float damageMarkerLifetime = 0.7f;

        [Space]
        [Header("Visibility")]
        public float visibilityCheckDistance = 60f;

        [Space]
        [Header("Debug")]
        public bool debugVisibilityRaycasts = true;
        public float debugRaycastDuration = 0.5f;
    }
}