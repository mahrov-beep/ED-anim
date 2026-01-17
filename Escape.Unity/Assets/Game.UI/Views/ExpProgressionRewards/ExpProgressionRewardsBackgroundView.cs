namespace Game.UI.Views.ExpProgressionRewards {
    using UniMob.UI;
    using Multicast;

    public class ExpProgressionRewardsBackgroundView : AutoView<IExpProgressionRewardsBackgroundState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("current_progress", () => this.State.CurrentProgress, 7f),
            this.Variable("max_progress", () => this.State.MaxProgress, 10f),
        };
    }

    public interface IExpProgressionRewardsBackgroundState : IViewState {
        float CurrentProgress { get; }
        float MaxProgress     { get; }
    }
}