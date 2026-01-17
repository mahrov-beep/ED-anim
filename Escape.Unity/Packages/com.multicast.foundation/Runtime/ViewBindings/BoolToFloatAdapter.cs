namespace Multicast {
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [AddComponentMenu("")]
    public class BoolToFloatAdapter : SingleResultAdapterBase<float, ViewVariableFloat> {
        [Space]
        [SerializeField]
        private ViewVariableBool source;

        [SerializeField]
        private float trueFloat = 1;

        [SerializeField]
        private float falseFloat = 0;

        protected override float Adapt() {
            return this.source.Value ? this.trueFloat : this.falseFloat;
        }
    }
}