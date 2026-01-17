namespace Multicast {
    using CodeWriter.ViewBinding;
    using JetBrains.Annotations;
    using Numerics;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [AddComponentMenu("View Binding/Adapters/[Binding] Cost Adapter")]
    public class CostAdapter : SingleResultAdapterBase<Cost, ViewVariableCost> {
        [SerializeField]
        private ViewVariableCost cost = default;

        [ShowInInspector, UsedImplicitly]
        [InlineProperty(LabelWidth = 150)]
        public ViewVariableCost.FormattingArgs Formatting {
            get => this.result.formatting;
            set => this.result.formatting = value;
        }

        [Required]
        [ShowInInspector, UsedImplicitly]
        [ViewContextCollection]
        public ViewContextBase[] ExtraContexts {
            get => this.result.extraContexts;
            set => this.result.extraContexts = value;
        }

        protected override Cost Adapt() {
            return this.cost.Value;
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();

            this.result.formatting.partsSeparator        = ViewVariableCost.FormattingArgs.DEFAULT_PARTS_SEPARATOR;
            this.result.formatting.currencyIconSeparator = ViewVariableCost.FormattingArgs.DEFAULT_CURRENCY_ICON_SEPARATOR;
            this.result.formatting.balanceSeparator      = ViewVariableCost.FormattingArgs.DEFAULT_BALANCE_SEPARATOR;
        }
#endif
    }
}