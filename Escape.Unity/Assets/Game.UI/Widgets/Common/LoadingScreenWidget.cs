namespace Game.UI.Widgets.Common {
    using UniMob.UI;
    using Views.Common;

    [RequireFieldsInit]
    public class LoadingScreenWidget : StatefulWidget {
        public WidgetViewReference View;
    }

    public class LoadingScreenState : ViewState<LoadingScreenWidget>, ILoadingScreenState {
        public override WidgetViewReference View => this.Widget.View;
    }
}