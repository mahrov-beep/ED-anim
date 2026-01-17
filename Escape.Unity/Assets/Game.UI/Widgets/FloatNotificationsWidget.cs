namespace Game.UI.Widgets {
    using Domain;
    using Multicast;
    using UniMob.UI;
    using Views;

    [RequireFieldsInit]
    public class FloatNotificationsWidget : StatefulWidget {
    }

    public class FloatNotificationsState : ViewState<FloatNotificationsWidget>, IFloatNotificationsState {
        [Inject] private SystemModel systemModel;

        public override WidgetViewReference View => UiConstants.Views.FloatNotificationsScreen;

        public bool NoInternetConnection => this.systemModel.IsInternetConnectionLost;
    }
}