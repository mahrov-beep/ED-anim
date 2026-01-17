namespace Game.UI.Widgets.RewardsLarge {
    using Domain.items;
    using Multicast;
    using Multicast.Numerics;
    using UniMob.UI;
    using Views.RewardLarge;

    [RequireFieldsInit]
    public class RewardLargeItemWidget : StatefulWidget {
        public Reward ItemReward;
    }

    public class RewardLargeItemState : ViewState<RewardLargeItemWidget>, IRewardLargeItemState {
        [Inject] private ItemsModel itemsModel;

        private ItemModel Model => this.itemsModel.Get(this.Widget.ItemReward.ItemKey);

        public override WidgetViewReference View => UiConstants.Views.RewardsLarge.Item;

        public string ItemKey  => this.Model.Key;
        public string ItemIcon => this.Model.ItemAsset.Icon;
    }
}