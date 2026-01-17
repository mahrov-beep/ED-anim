namespace Game.UI.Widgets.ItemInfo {
    using System;
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using Views.ItemInfo;

    [RequireFieldsInit]
    public class ItemInfoUsedStatWidget : StatefulWidget {
        public ERarityType StatRarity;
        public int         UsagesRemaining;
        public int         MaxUsages;
    }

    public class ItemInfoUsedStatState : ViewState<ItemInfoUsedStatWidget>, IItemInfoStatState {
        public override WidgetViewReference View => UiConstants.Views.ItemInfo.Stat;


        public string StatRarity => EnumNames<ERarityType>.GetName(this.Widget.StatRarity);

        public string StatKey => "USED";

        public string StatValue {
            get {
                var maxUsages = this.Widget.MaxUsages;
                var remUses   = this.Widget.UsagesRemaining;

                return remUses == 0 ? "Empty" : $"{remUses}/{maxUsages}";
            }
        }
    }
}