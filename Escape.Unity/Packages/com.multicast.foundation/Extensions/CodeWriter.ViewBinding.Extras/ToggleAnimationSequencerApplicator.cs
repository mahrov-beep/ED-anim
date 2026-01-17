#if ANIMATION_SEQUENCER
namespace CodeWriter.ViewBinding.Extras {
    using System;
    using Applicators;
    using BrunoMikoski.AnimationSequencer;
    using Multicast;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Serialization;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AnimationSequencerController))]
    [AddComponentMenu("View Binding/UI/[Binding] Toggle Animation Sequencer Applicator")]
    public class ToggleAnimationSequencerApplicator : ComponentApplicatorBase<AnimationSequencerController, ViewVariableBool>, IAutoViewListener {
        [SerializeField]
        [FormerlySerializedAs("rewindOnEnable")]
        private bool rewindOnActivate = true;

        void IAutoViewListener.Activate() {
            if (this.rewindOnActivate) {
                this.GetTarget().Rewind();
            }
        }

        void IAutoViewListener.Deactivate() {
        }

        protected override void Apply(AnimationSequencerController target, ViewVariableBool source) {
            var play = source.Value;

            using (Atom.NoWatch) {
                if (Application.isPlaying) {
                    if (play) {
                        target.PlayForward(resetFirst: false);
                    }
                    else {
                        target.PlayBackwards(completeFirst: false);
                    }
                }
            }
        }
    }
}

#endif