namespace Game.UI.Widgets.RewardsLarge {
    using Multicast.Numerics;
    using UniMob.UI;
    using Views.RewardLarge;

    [RequireFieldsInit]
    public class RewardLargeCurrencyWidget : StatefulWidget {
        public Reward CurrencyReward;
    }

    public class RewardLargeCurrencyState : ViewState<RewardLargeCurrencyWidget>, IRewardLargeCurrencyState {
        public override WidgetViewReference View => UiConstants.Views.RewardsLarge.Currency;

        public string CurrencyKey => this.Widget.CurrencyReward.ItemKey;
    }
}