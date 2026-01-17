namespace Multicast {
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [AddComponentMenu("")]
    public class FloatToMultiplierAdapter : SingleResultAdapterBase<float, ViewVariableFloat> {
        [SerializeField] private int precision = 100;

        [Space]
        [SerializeField]
        private ViewVariableFloat source = default;

        protected override float Adapt() {
            return Mathf.Round((1 + this.source.Value) * this.precision) / this.precision;
        }
    }
}