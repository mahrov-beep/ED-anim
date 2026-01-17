namespace Game.UI.Widgets.ItemInfo {
    using Multicast;
    using Multicast.Numerics;
    using Photon.Deterministic;
    using Quantum;
    using UniMob.UI;
    using Views.ItemInfo;

    [RequireFieldsInit]
    public class ItemInfoStatWidget : StatefulWidget {
        public EAttributeType StatType;
        public BigDouble      StatValue;
        public ERarityType    StatRarity;
        public FP             Duration;
    }

    public class ItemInfoStatState : ViewState<ItemInfoStatWidget>, IItemInfoStatState {
        public override WidgetViewReference View => UiConstants.Views.ItemInfo.Stat;

        public string StatKey    => EnumNames<EAttributeType>.GetName(this.Widget.StatType);
        public string StatRarity => EnumNames<ERarityType>.GetName(this.Widget.StatRarity);

        public string StatValue {
            get {
                var value         = this.Widget.StatValue;
                var valueWithSign = value > 0 ? $"+{BigString.ToString(value)}" : $"{BigString.ToString(value)}";
                var duration      = this.Widget.Duration;

                return EnumNames<EAttributeType>.GetName(this.Widget.StatType) switch {
                    var additive when additive.StartsWith("AdditiveBoost_") && duration > 0 => $"{valueWithSign} for {duration}s",
                    var additive when additive.StartsWith("AdditiveBoost_") => valueWithSign,
                    
                    var percent when percent.StartsWith("PercentBoost_") && duration > 0 => valueWithSign + $"% for {duration}s",
                    var percent when percent.StartsWith("PercentBoost_") => valueWithSign + "%",
                    
                    _ => valueWithSign,
                };
            }
        }
    }
}