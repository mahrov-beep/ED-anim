namespace CodeWriter.ViewBinding.Extras {
    using TriInspector;
    using UniMob;
    using UnityEngine;

    [RequireComponent(typeof(ParticleSystem))]
    public class PlayParticleSystem : ApplicatorBase {
        [SerializeField] private ViewEventVoid playEvent = default;

        [SerializeField, Required] private ParticleSystem system = default;

        protected override void Setup(Lifetime lifetime) {
            if (lifetime.IsDisposed) {
                return;
            }

            this.playEvent.AddListener(this.Play);

            lifetime.Register(() => this.playEvent.RemoveListener(this.Play));
        }

        protected override void LinkToRender() {
        }

        protected override void Apply() {
        }

        private void Play() {
            this.system.Play();
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();

            this.system = this.GetComponent<ParticleSystem>();
        }
#endif
    }
}