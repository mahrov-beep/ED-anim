namespace Game.UI.Widgets.Game {
    using Domain.Game;
    using Multicast;
    using UniMob.UI;
    using Views.Game;

    [RequireFieldsInit]
    public class GameEscapeModeSummaryWidget : StatefulWidget {
    }

    public class GameEscapeModeSummaryState : ViewState<GameEscapeModeSummaryWidget>, IGameEscapeModeSummaryState {
        [Inject] private GameStateModel gameStateModel;

        public override WidgetViewReference View => UiConstants.Views.Game.EscapeModeSummary;

        public int GameSecondsLeft => this.gameStateModel.SecondsToStateEnd;
    }
}