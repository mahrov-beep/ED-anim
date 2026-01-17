namespace Game.UI.Widgets.Common {
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Common;

    public class AlertDialogButtonWidget : StatefulWidget {
        public string Text     { get; }
        public bool   Positive { get; }

        public AlertDialogButtonWidget(string text, bool positive) {
            this.Text     = text;
            this.Positive = positive;
        }
    }

    public class AlertDialogButtonState : ViewState<AlertDialogButtonWidget>, IAlertDialogButtonState {
        public override WidgetViewReference View => this.Widget.Positive
            ? UiConstants.Views.Alert.ButtonPositive
            : UiConstants.Views.Alert.ButtonNegative;

        public string ButtonKey      => this.Widget.Text;
        public bool   IsInteractable => true;

        public void Click() {
            Navigator.Of(this.Context).Pop(this.Widget.Positive);
        }
    }
}