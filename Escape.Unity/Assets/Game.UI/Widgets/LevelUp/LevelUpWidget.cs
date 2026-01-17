namespace Game.UI.Widgets.LevelUp {
    using System;
    using Domain.ExpProgressionRewards;
    using Multicast;
    using RewardsLarge;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.LevelUp;

    [RequireFieldsInit]
    public class LevelUpWidget : StatefulWidget {
        public int PrevLevel;
        public int NextLLevel;

        public Action OnContinue;
    }

    public class LevelUpState : ViewState<LevelUpWidget>, ILevelUpState {
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;

        private readonly StateHolder rewardsState;

        public LevelUpState() {
            this.rewardsState = this.CreateChild(this.BuildRewards);
        }

        public override WidgetViewReference View => UiConstants.Views.LevelUp.Screen;

        public int PrevLevel => this.Widget.PrevLevel;
        public int NextLevel => this.Widget.NextLLevel;

        public IState Rewards => this.rewardsState.Value;

        public void Continue() {
            this.Widget.OnContinue?.Invoke();
        }

        private Widget BuildRewards(BuildContext context) {
            var expProgressionReward = this.expProgressionRewardsModel.FirstLocked;
            if (expProgressionReward?.LevelToComplete == this.Widget.PrevLevel) {
                return new RewardLargeRowWidget {
                    Rewards            = expProgressionReward.RewardsPreview,
                    MainAxisSize       = AxisSize.Max,
                    CrossAxisSize      = AxisSize.Max,
                    MainAxisAlignment  = MainAxisAlignment.Center,
                    CrossAxisAlignment = CrossAxisAlignment.Center,
                };
            }

            return new Empty();
        }
    }
}