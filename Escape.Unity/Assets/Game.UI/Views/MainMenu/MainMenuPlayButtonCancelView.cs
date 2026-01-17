namespace Game.UI.Views.MainMenu {
    using Multicast;
    using UniMob.UI;
    using Views.Common;

    public class MainMenuPlayButtonCancelView : AutoView<IMainMenuPlayButtonState> {
        private readonly DotsAnimator dots = new DotsAnimator();

        protected override AutoViewEventBinding[] Events => new[] {            
            this.Event("stop", () => this.State.Stop()),
        };

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("is_searching_match", () => this.State.IsSearchingMatch, false),
            this.Variable("is_ready", () => this.State.IsReady),
            this.Variable("is_leader", () => this.State.IsLeader),
            this.Variable("matchmaking_time", () => this.State.MatchmakingTimeRemaining, 0),
            this.Variable("dots", () => this.dots.Value, "..."),
        };

        protected override void Activate() {
            base.Activate();

            this.dots.Activate(this.HasState && this.State.IsSearchingMatch);
        }

        private void Update() {
            if (!this.HasState) {
                return;
            }

            this.dots.Update(this.State.IsSearchingMatch);
        }
    }
}
