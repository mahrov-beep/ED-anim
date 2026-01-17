namespace Multicast {
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [AddComponentMenu("View Binding/Adapters/[Binding] BigDouble Ratio Adapter")]
    public sealed class BigDoubleRatioAdapter : SingleResultAdapterBase<float, ViewVariableFloat> {
        [Space]
        [SerializeField]
        private ViewVariableBigDouble numerator;

        [SerializeField]
        private ViewVariableBigDouble denominator;

        protected override float Adapt() {
            var n = this.numerator.Value;
            var d = this.denominator.Value;

            if (d < 0.0001f) {
                return 0f;
            }

            var r = (n / d).ToFloatUnsafe();

            if (float.IsNaN(r) || float.IsInfinity(r)) {
                Debug.LogError($"[BigDoubleRatioAdapter] Failed to divide '{n}' / '{r}'", this);
                r = 0f;
            }

            return r;
        }
    }
}