#if ANIMATION_SEQUENCER
namespace CodeWriter.ViewBinding.Extras {
    using System;
    using BrunoMikoski.AnimationSequencer;
    using Multicast;
    using TriInspector;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Serialization;

    [RequireComponent(typeof(AnimationSequencerController))]
    public class PlayAnimationSequencer : ApplicatorBase, IAutoViewListener {
        [SerializeField] private ViewEventVoid playEvent = default;

        [SerializeField, Required] private AnimationSequencerController controller = default;

        [SerializeField]
        [FormerlySerializedAs("resetOnAwake")]
        private bool resetOnActivate = true;

        void IAutoViewListener.Activate() {
            if (this.resetOnActivate) {
                this.controller.PlayBackwards(completeFirst: false);
            }
        }

        void IAutoViewListener.Deactivate() {
        }

        protected override void Setup(Lifetime lifetime) {
            this.playEvent.AddListener(this.Play);

            lifetime.Register(() => this.playEvent.RemoveListener(this.Play));
        }

        protected override void LinkToRender() {
        }

        protected override void Apply() {
        }

        private void Play() {
            this.controller.PlayForward();
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();

            this.controller = this.GetComponent<AnimationSequencerController>();
        }
#endif
    }
}

#endif