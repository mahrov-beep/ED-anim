namespace _Project.Scripts.MapBaker {
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [RequireComponent(typeof(QuantumMapData))]
    public class MinimapBakerTool : MonoBehaviour {
        [Button]
        public void BakeLevelToSprite() {
            var quantumMap = this.GetComponent<QuantumMapData>();

            MinimapBakerProcessor.BakeLevelToSprite(quantumMap);
        }
    }
}