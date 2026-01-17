namespace Game.ECS.Systems.Unit {
    using UnityEngine;

    public class DebuffVisual : MonoBehaviour {
        [field: SerializeField]
        public ParticleSystemWrapper SlowDebuff { get; private set; }

        [field: SerializeField]
        public ParticleSystemWrapper BurnDebuff { get; private set; }
    }
}