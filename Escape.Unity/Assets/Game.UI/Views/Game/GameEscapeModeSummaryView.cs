namespace Game.UI.Views.Game {
    using UniMob.UI;
    using Multicast;

    public class GameEscapeModeSummaryView : AutoView<IGameEscapeModeSummaryState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("game_seconds_left", () => this.State.GameSecondsLeft, 61),
        };
    }

    public interface IGameEscapeModeSummaryState : IViewState {
        int GameSecondsLeft { get; }
    }
}