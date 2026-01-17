namespace Game.UI.Widgets.Notifier {
    using UniMob.UI;
    using Views.Notifier;

    [RequireFieldsInit]
    public class NotifierWidget : StatefulWidget {
        public int  Counter;
        public bool IsNew;
    }

    public class NotifierState : ViewState<NotifierWidget>, INotifierState {
        public override WidgetViewReference View { get; }

        public int Notifier => this.Widget.Counter;

        public bool IsNew => this.Widget.IsNew;
    }
}