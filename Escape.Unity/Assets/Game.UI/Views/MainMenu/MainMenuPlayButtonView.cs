namespace Game.UI.Views.MainMenu {
    using Multicast;
    using UniMob.UI;

    public class MainMenuPlayButtonView : AutoView<IMainMenuPlayButtonState> {
        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("play", () => this.State.Play()),
            this.Event("stop", () => this.State.Stop()),
        };

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("is_searching_match", () => this.State.IsSearchingMatch, false),
            this.Variable("is_ready", () => this.State.IsReady),
            this.Variable("is_leader", () => this.State.IsLeader),
            this.Variable("matchmaking_time", () => this.State.MatchmakingTimeRemaining, 0),
        };
    }

    public interface IMainMenuPlayButtonState : IViewState {
        bool IsSearchingMatch { get; }
        bool IsLeader         { get; }
        bool IsReady          { get; }

        int MatchmakingTimeRemaining { get; }

        void Play();
        void Stop();
    }
}

