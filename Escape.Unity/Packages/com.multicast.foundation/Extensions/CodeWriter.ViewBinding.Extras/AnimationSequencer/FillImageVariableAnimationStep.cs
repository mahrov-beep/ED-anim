namespace Multicast {
    using System;
    using BrunoMikoski.AnimationSequencer;
    using CodeWriter.ViewBinding;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    [Serializable]
    public class FillImageVariableAnimationStep : AnimationStepBase {
        [SerializeField] private Image target;
        [SerializeField] private float duration = 1f;

        [SerializeField] protected CustomEase ease = CustomEase.Linear;

        [SerializeField] private ViewVariableFloat fillAmountFrom;
        [SerializeField] private ViewVariableFloat fillAmountTo;

        public ViewVariableFloat FillAmountFrom => this.fillAmountFrom;
        public ViewVariableFloat FillAmountTo   => this.fillAmountTo;

        private float previousFillAmount;

        public override string DisplayName { get; } = "Fill Image by Variable";

        public override void AddTweenToSequence(Sequence animationSequence) {
            var sequence = DOTween.Sequence();

            sequence.SetDelay(this.Delay);
            sequence.Append(this.GenerateTween_Internal());

            if (this.FlowType == FlowType.Join) {
                animationSequence.Join(sequence);
            }
            else {
                animationSequence.Append(sequence);
            }
        }

        public override void ResetToInitialState() {
            if (this.target == null) {
                return;
            }

            this.target.fillAmount = this.previousFillAmount;
        }

        private Tweener GenerateTween_Internal() {
            if (this.target == null) {
                Debug.LogError($"{nameof(FillImageVariableAnimationStep)} target image is null");
                return null;
            }

            this.previousFillAmount = this.target.fillAmount;

            var tween = DOTween.To(
                getter: () => 0f,
                setter: v => this.target.fillAmount = Mathf.LerpUnclamped(this.fillAmountFrom.Value, this.fillAmountTo.Value, v),
                endValue: 1f,
                duration: this.duration);

            tween.SetTarget(this.target);
            tween.SetEase(this.ease);

#if UNITY_EDITOR
            // https://forum.unity.com/threads/editor-scripting-force-color-update.798663/
            tween.OnUpdate(() => {
                if (!Application.isPlaying && this.target.enabled) {
                    this.target.enabled = false;
                    this.target.enabled = true;
                }
            });
#endif

            return tween;
        }

        public override string GetDisplayNameForEditor(int index) {
            var targetName = this.target != null ? this.target.name : "[NULL]";

            if (this.fillAmountFrom.Name == this.fillAmountTo.Name) {
                return $"{index}. {this.DisplayName}: {targetName} :: {this.fillAmountTo.Name}";
            }

            return $"{index}. {this.DisplayName}: {targetName} :: {this.fillAmountFrom.Name} -> {this.fillAmountTo.Name}";
        }
    }
}