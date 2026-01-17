namespace Multicast {
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [AddComponentMenu("")]
    public class FloatToPercentAdapter : SingleResultAdapterBase<float, ViewVariableFloat> {
        [Space]
        [SerializeField]
        private ViewVariableFloat source = default;

        protected override float Adapt() {
            return Mathf.RoundToInt(this.source.Value * 100);
        }
    }
}