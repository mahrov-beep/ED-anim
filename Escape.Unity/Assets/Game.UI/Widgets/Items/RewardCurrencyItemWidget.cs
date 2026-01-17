namespace Game.UI.Widgets.Items {
    using Multicast.Numerics;
    using UniMob.UI;
    using Views.Items;

    [RequireFieldsInit]
    public class RewardCurrencyItemWidget : StatefulWidget {
        public Reward Reward;
    }

    public class RewardCurrencyItemState : ViewState<RewardCurrencyItemWidget>, ICurrencyItemState {
        public override WidgetViewReference View => UiConstants.Views.Items.CurrencyItem;

        public string CurrencyKey    => this.Widget.Reward.ItemKey;
        public int    CurrencyAmount => this.Widget.Reward.IntAmount;
    }
}