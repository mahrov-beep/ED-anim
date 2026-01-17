namespace CodeWriter.ViewBinding.Applicators.Adapters {
    using UnityEngine;

    [AddComponentMenu("View Binding/Adapters/[Binding] Int Ratio Adapter")]
    public class IntRatioAdapter : SingleResultAdapterBase<float, ViewVariableFloat> {
        [Space]
        [SerializeField]
        private ViewVariableInt numerator;

        [SerializeField]
        private ViewVariableInt denominator;

        [SerializeField]
        private float onNan = 1f;

        protected override float Adapt() {
            return Mathf.Approximately(this.denominator.Value, 0f)
                ? this.onNan
                : 1f * this.numerator.Value / this.denominator.Value;
        }
    }
}