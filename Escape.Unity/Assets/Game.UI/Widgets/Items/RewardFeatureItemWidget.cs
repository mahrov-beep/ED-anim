namespace Game.UI.Widgets.Items {
    using Multicast.Numerics;
    using UniMob.UI;
    using Views.items;

    [RequireFieldsInit]
    public class RewardFeatureItemWidget : StatefulWidget {
        public Reward Reward;
    }

    public class RewardFeatureItemState : ViewState<RewardFeatureItemWidget>, IFeatureItemState {
        public override WidgetViewReference View => UiConstants.Views.Items.FeatureItem;

        public string FeatureKey => this.Widget.Reward.ItemKey;
    }
}