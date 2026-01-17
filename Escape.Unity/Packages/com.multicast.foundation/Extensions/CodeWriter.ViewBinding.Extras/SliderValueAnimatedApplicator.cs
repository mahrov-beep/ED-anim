using UnityEngine;
using UnityEngine.UI;

namespace CodeWriter.ViewBinding.Applicators.UI {
    using Multicast;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Slider))]
    [AddComponentMenu("View Binding/UI/[Binding] Slider Value Animated Applicator")]
    public sealed class SliderValueAnimatedApplicator : ComponentApplicatorBase<Slider, ViewVariableFloat>, IAutoViewListener {
        [SerializeField] private float positiveAnimationDuration = 0.2f;
        [SerializeField] private float negativeAnimationDuration = 0.2f;
        [SerializeField] private float delay                     = 0.2f;

        private Slider currentTarget;
        private float  targetValue;
        private float  animationMaxDelta;
        private float  remainingDelay;

        void IAutoViewListener.Activate() {
            this.currentTarget = null;

            this.Apply();
        }

        void IAutoViewListener.Deactivate() {
            if (this.currentTarget != null) {
                this.currentTarget.value = this.targetValue;
            }

            this.currentTarget     = null;
            this.animationMaxDelta = 0;
        }

        protected override void Apply(Slider target, ViewVariableFloat source) {
            var isFirstRun = this.currentTarget == null;
            var distance   = source.Value - target.value;
            var duration   = distance > 0 ? this.positiveAnimationDuration : this.negativeAnimationDuration;

            this.targetValue = source.Value;

            if (this.gameObject.activeInHierarchy) {
                this.currentTarget = target;

                if (isFirstRun || Mathf.Approximately(duration, 0f)) {
                    target.value = this.targetValue;
                }
                else {
                    this.remainingDelay    = this.delay;
                    this.animationMaxDelta = Mathf.Abs(distance) / duration;
                }
            }
            else {
                target.value = this.targetValue;
            }
        }

        private void Update() {
            if (this.animationMaxDelta == 0) {
                return;
            }

            if (this.remainingDelay > 0f) {
                this.remainingDelay -= Time.unscaledDeltaTime;
                return;
            }

            this.currentTarget.value = Mathf.MoveTowards(this.currentTarget.value, this.targetValue,
                Time.unscaledDeltaTime * this.animationMaxDelta);

            if (Mathf.Approximately(this.currentTarget.value, this.targetValue)) {
                this.currentTarget.value = this.targetValue;
                this.animationMaxDelta   = 0;
            }
        }
    }
}