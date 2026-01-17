namespace Game.UI.Widgets.Purchases.Drops {
    using Multicast.Numerics;
    using Multicast.RewardSystem;
    using Shared.Defs;
    using UniMob.UI;
    using Views.Purchases.Drops;

    public class PurchasesCurrencyDropWidget : StatefulWidget {
        public RewardDef RewardDef     { get; }
        public string    CategoryKey { get; }
        public string    PurchaseKey { get; }

        public WidgetViewReference? ViewReference { get; set; }

        public PurchasesCurrencyDropWidget(RewardDef rewardDef, string categoryKey, string purchaseKey) {
            this.RewardDef   = rewardDef;
            this.CategoryKey = categoryKey;
            this.PurchaseKey = purchaseKey;
        }
    }

    public class PurchasesCurrencyDropState : ViewState<PurchasesCurrencyDropWidget>, IPurchasesCurrencyDropState {
        public override WidgetViewReference View => this.Widget.ViewReference.GetValueOrDefault(this.GetDefaultView());

        public string    CategoryKey => this.Widget.CategoryKey;
        public string    PurchaseKey => this.Widget.PurchaseKey;
        public string    CurrencyKey => this.CurrencyRewardDef.currency;
        public BigDouble Amount      => this.ParseAmount();


        private CurrencyRewardDef CurrencyRewardDef => this.Widget.RewardDef as CurrencyRewardDef;

        private BigDouble ParseAmount() {
            if (this.CurrencyRewardDef != null) {
                return this.CurrencyRewardDef.amount;
            }

            return BigDouble.Zero;
        }

        private WidgetViewReference GetDefaultView() {
            if (this.PurchaseKey.StartsWith("Crystal_Pack_")) {
                return UiConstants.Views.Purchases.Drops.CrystalPack;
            }

            return UiConstants.Views.Purchases.Drops.Currency;
        }
    }
}