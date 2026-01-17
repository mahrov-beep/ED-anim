namespace Game.UI.Widgets.ItemInfo {
    using Multicast;
    using Quantum;
    using Quantum.Prototypes;
    using UniMob.UI;
    using Views.ItemInfo;

    [RequireFieldsInit]
    public class ItemInfoHealStatWidget : StatefulWidget {
        public ERarityType               StatRarity;
        public HealthApplicatorPrototype Heal;
    }

    public class ItemInfoHealStatState : ViewState<ItemInfoHealStatWidget>, IItemInfoStatState {
        public override WidgetViewReference View => UiConstants.Views.ItemInfo.Stat;

        public string StatRarity => EnumNames<ERarityType>.GetName(this.Widget.StatRarity);
        public string StatKey    => "HEAL";

        public string StatValue {
            get {
                var heal = this.Widget.Heal;

                return (HealthAttributeAppliance)heal.Appliance switch {
                    HealthAttributeAppliance.OneTime when heal.ValueIsPercent => $"+{heal.Value}%",
                    HealthAttributeAppliance.OneTime => $"+{heal.Value}hp",

                    HealthAttributeAppliance.Continuous when heal.ValueIsPercent => $"+{heal.Value}% in {heal.Duration}s",
                    HealthAttributeAppliance.Continuous => $"+{heal.Value}hp in {heal.Duration}s",

                    HealthAttributeAppliance.Temporary when heal.ValueIsPercent => $"+{heal.Value}% for {heal.Duration}s",
                    HealthAttributeAppliance.Temporary => $"+{heal.Value}hp for {heal.Duration}s",

                    _ => "INVALID",
                };
            }
        }
    }
}