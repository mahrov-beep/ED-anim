namespace Game.UI.Widgets.Purchases {
    using System.Collections.Generic;
    using System.Linq;
    using Drops;
    using Multicast.DropSystem;
    using Multicast.RewardSystem;
    using Shared.Defs;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    public class PurchasesDropWidget : StatefulWidget {
        public List<RewardDef> Rewards       { get; }
        public string          PurchaseKey { get; }
        public string          CategoryKey { get; }

        public PurchasesDropWidget(List<RewardDef> rewards, string purchaseKey, string categoryKey) {
            this.Rewards     = rewards;
            this.PurchaseKey = purchaseKey;
            this.CategoryKey = categoryKey;
        }
    }

    public class PurchasesDropState : HocState<PurchasesDropWidget> {
        public override Widget Build(BuildContext context) {
            return this.BuildSingleReward();
        }
        
        private Widget BuildSingleReward() => this.Widget.Rewards.FirstOrDefault() switch {
            CurrencyRewardDef currency => this.BuildCurrencyRewardItem(currency),
            _ => new Empty(),
        };

        private Widget BuildCurrencyRewardItem(RewardDef currencyRewardDef) {
            return new PurchasesCurrencyDropWidget(currencyRewardDef, this.Widget.CategoryKey, this.Widget.PurchaseKey);
        }
    }
}