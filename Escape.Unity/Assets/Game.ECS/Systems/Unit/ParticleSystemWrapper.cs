namespace Game.ECS.Systems.Unit {
    using UnityEngine;

    [System.Serializable]
    public class ParticleSystemWrapper {
        [SerializeField, Sirenix.OdinInspector.Required]
        private ParticleSystem particleSystem;

        [Header("Play(withChildren) || Stop(withChildren)")]
        [SerializeField]
        private bool withChildren = true;

        [SerializeField]
        private ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting;

        [Sirenix.OdinInspector.ShowInInspector]
        [Sirenix.OdinInspector.PropertyOrder(-1)]
        [Sirenix.OdinInspector.TableColumnWidth(20, false)]
        public bool On {
            get => on;
            set {
                if (value && !on) {
                    particleSystem.Play(withChildren: true);
                    on = true;
                }
                else if (!value && on) {
                    particleSystem.Stop(withChildren: true, stopBehavior);
                    on = false;
                }
            }
        }

        private bool on;

        public void Play() => On = true;
        public void Stop() => On = false;
    }
}