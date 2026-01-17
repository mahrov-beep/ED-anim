namespace Game.UI.Widgets.MainMenu {
    using Controllers.Features.ExpProgressionRewards;
    using Domain.ExpProgressionRewards;
    using Multicast;
    using Rewards;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.MainMenu;

    [RequireFieldsInit]
    public class MainMenuExpProgressionRewardsWidget : StatefulWidget {
    }

    public class MainMenuExpProgressionRewardsState : ViewState<MainMenuExpProgressionRewardsWidget>, IMainMenuExpProgressionRewardsState {
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;

        private readonly StateHolder rewardsState;

        public MainMenuExpProgressionRewardsState() {
            this.rewardsState = this.CreateChild(this.BuildRewards);
        }

        public override WidgetViewReference View => default;

        public IState Rewards => this.rewardsState.Value;

        public void Open() {
            ExpProgressionRewardsFeatureEvents.Open.Raise();
        }

        private Widget BuildRewards(BuildContext context) {
            return new RewardsRowWidget {
                Rewards = this.expProgressionRewardsModel.RewardsPreviewForMainMenu,
            };
        }
    }
}