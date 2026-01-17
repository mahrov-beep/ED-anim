namespace Game.UI.Widgets.Common {
    using MainMenu;
    using Multicast;
    using UniMob.UI;
    using UniMob;
    using Views.Common;
    using Views.MainMenu;

    public class SearchGameScreenWidget : StatefulWidget {
    }

    public class SearchGameScreenState : ViewState<SearchGameScreenWidget>, ISearchGameScreenState {
        public override WidgetViewReference View => UiConstants.Views.SearchGameScreen;

        [Atom]
        public IMainMenuPlayButtonState PlayButton =>
            this.RenderChildT(_ => new MainMenuPlayButtonCancelWidget()).As<MainMenuPlayButtonCancelState>();
    }
}
