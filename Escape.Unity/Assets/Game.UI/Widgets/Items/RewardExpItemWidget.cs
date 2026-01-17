namespace Game.UI.Widgets.Items {
    using Multicast.Numerics;
    using UniMob.UI;
    using Views.items;

    [RequireFieldsInit]
    public class RewardExpItemWidget : StatefulWidget {
        public Reward Reward;
    }

    public class RewardExpItemState : ViewState<RewardExpItemWidget>, IExpItemState {
        public override WidgetViewReference View => UiConstants.Views.Items.ExpItem;

        public string ExpKey    => this.Widget.Reward.ItemKey;
        public int    ExpAmount => this.Widget.Reward.IntAmount;
    }
}