namespace CodeWriter.ViewBinding.Applicators.UI {
    using UniMob;
    using UnityEngine;

    [DisallowMultipleComponent]
    [AddComponentMenu("View Binding/UI/Adapters/[Binding] Animated Integer Adapter")]
    public class AnimatedIntAdapter : SingleResultAdapterBase<int, ViewVariableInt> {
        [Space]
        [SerializeField]
        private ViewVariableInt source;

        [SerializeField] private float positiveAnimationDuration = 0.2f;
        [SerializeField] private float negativeAnimationDuration = 0.2f;
        [SerializeField] private float delay                     = 0.2f;

        private readonly MutableAtom<float> value = Atom.Value(0f);

        private bool  wasRun;
        private float targetValue;
        private float animationMaxDelta;
        private float remainingDelay;

        private void OnDisable() {
            this.wasRun            = false;
            this.animationMaxDelta = 0f;
        }

        private void Update() {
            if (this.animationMaxDelta == 0) {
                return;
            }

            if (this.remainingDelay > 0f) {
                this.remainingDelay -= Time.unscaledDeltaTime;
                return;
            }

            this.value.Value = Mathf.MoveTowards(this.value.Value, this.targetValue,
                Time.unscaledDeltaTime * this.animationMaxDelta);

            if (Mathf.Approximately(this.value.Value, this.targetValue)) {
                this.value.Value       = this.targetValue;
                this.animationMaxDelta = 0;
            }
        }

        protected override int Adapt() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return this.source.Value;
            }
#endif

            var distance = this.source.Value - this.value.Value;
            var duration = distance > 0 ? this.positiveAnimationDuration : this.negativeAnimationDuration;

            using (Atom.NoWatch) {
                this.targetValue = this.source.Value;

                if (!this.wasRun || Mathf.Approximately(duration, 0f)) {
                    this.wasRun      = true;
                    this.value.Value = this.targetValue;
                }
                else {
                    this.remainingDelay    = this.delay;
                    this.animationMaxDelta = Mathf.Abs(distance) / duration;
                }
            }

            return Mathf.RoundToInt(this.value.Value);
        }
    }
}