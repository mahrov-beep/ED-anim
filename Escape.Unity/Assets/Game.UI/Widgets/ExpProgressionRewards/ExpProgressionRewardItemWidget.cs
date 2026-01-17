namespace Game.UI.Widgets.ExpProgressionRewards {
    using Domain.ExpProgressionRewards;
    using Multicast;
    using RewardsLarge;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Views.ExpProgressionRewards;

    [RequireFieldsInit]
    public class ExpProgressionRewardItemWidget : StatefulWidget {
        public string ExpProgressionRewardKey;
    }

    public class ExpProgressionRewardItemState : ViewState<ExpProgressionRewardItemWidget>, IExpProgressionRewardItemState {
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;

        private readonly StateHolder rewardState;

        public ExpProgressionRewardItemState() {
            this.rewardState = this.CreateChild(this.BuildReward);
        }

        private ExpProgressionRewardModel Model => this.expProgressionRewardsModel.Get(this.Widget.ExpProgressionRewardKey);

        public override WidgetViewReference View => UiConstants.Views.ExpProgressionRewards.Item;

        public IState Reward => this.rewardState.Value;

        public int Level => this.Model.LevelToComplete;

        public bool Selected => this.expProgressionRewardsModel.Selected == this.Model;
        public bool CanClaim => this.Model.IsUnlocked && !this.Model.IsClaimed;
        public bool Claimed  => this.Model.IsClaimed;

        public override WidgetSize CalculateSize() {
            var (minW, minH, maxW, maxH) = base.CalculateSize();
            var w = minW * this.Model.PlacesTakenInRewardsRow;
            return new WidgetSize(w, minH, w, maxH);
        }

        public void Select() {
            this.expProgressionRewardsModel.Selected = this.Model;
        }

        private Widget BuildReward(BuildContext context) {
            return new RewardLargeRowWidget {
                Rewards            = this.Model.RewardsPreview,
                CrossAxisSize      = AxisSize.Max,
                CrossAxisAlignment = CrossAxisAlignment.Center,
            };
        }
    }
}