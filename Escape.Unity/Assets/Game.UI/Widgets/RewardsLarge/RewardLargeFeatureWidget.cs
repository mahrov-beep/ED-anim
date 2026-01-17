namespace Game.UI.Widgets.RewardsLarge {
    using Multicast.Numerics;
    using UniMob.UI;
    using Views.RewardLarge;

    [RequireFieldsInit]
    public class RewardLargeFeatureWidget : StatefulWidget {
        public Reward FeatureReward;
    }

    public class RewardLargeFeatureState : ViewState<RewardLargeFeatureWidget>, IRewardLargeFeatureState {
        public override WidgetViewReference View => UiConstants.Views.RewardsLarge.Feature;

        public string FeatureKey => this.Widget.FeatureReward.ItemKey;
    }
}