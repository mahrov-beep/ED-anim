namespace Multicast {
    using System;
    using BrunoMikoski.AnimationSequencer;
    using CodeWriter.ViewBinding;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Serializable, Preserve]
    public sealed class InvokeViewEventVoidAnimationStep : AnimationStepBase {
        [SerializeField]
        private ViewEventVoid viewEvent;

        public ViewEventVoid ViewEvent => this.viewEvent;

        public override string DisplayName => "Invoke ViewEventVoid";

        public override void AddTweenToSequence(Sequence animationSequence) {
            var sequence = DOTween.Sequence();

            sequence.SetDelay(this.Delay);
            sequence.AppendCallback(this.Invoke);

            if (this.FlowType == FlowType.Append) {
                animationSequence.Append(sequence);
            }
            else {
                animationSequence.Join(sequence);
            }
        }

        public override void ResetToInitialState() {
        }

        public override string GetDisplayNameForEditor(int index) {
            return $"{index}. {this.DisplayName}: {this.viewEvent.Name}";
        }

        private void Invoke() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            this.viewEvent.Invoke();
        }
    }
}