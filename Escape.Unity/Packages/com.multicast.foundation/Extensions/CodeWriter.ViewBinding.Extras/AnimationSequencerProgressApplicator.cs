#if ANIMATION_SEQUENCER

namespace CodeWriter.ViewBinding.Extras {
    using Applicators;
    using BrunoMikoski.AnimationSequencer;
    using UniMob;
    using UnityEngine;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AnimationSequencerController))]
    [AddComponentMenu("View Binding/UI/[Binding] Animation Sequencer Progress Applicator")]
    public class AnimationSequencerProgressApplicator : ComponentApplicatorBase<AnimationSequencerController, ViewVariableFloat> {
        protected override void Apply(AnimationSequencerController target, ViewVariableFloat source) {
            var value = source.Value;

            using (Atom.NoWatch) {
                if (Application.isPlaying) {
                    target.SetProgress(value, andPlay: false);
                }
            }
        }
    }
}
#endif