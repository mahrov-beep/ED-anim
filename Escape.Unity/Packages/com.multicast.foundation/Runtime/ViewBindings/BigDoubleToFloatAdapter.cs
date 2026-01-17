namespace Multicast {
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [AddComponentMenu("View Binding/Adapters/[Binding] BigDouble to Float Adapter")]
    public class BigDoubleToFloatAdapter : SingleResultAdapterBase<float, ViewVariableFloat> {
        [Space]
        [SerializeField]
        private ViewVariableBigDouble source = default;

        protected override float Adapt() {
            return this.source.Value.ToFloatUnsafe();
        }
    }
}