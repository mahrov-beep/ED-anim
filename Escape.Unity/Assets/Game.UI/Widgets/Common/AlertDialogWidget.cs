namespace Game.UI.Widgets.Common {
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Common;

    public class AlertDialogWidget : StatefulWidget {
        public string   Title       { get; private set; }
        public string   Text        { get; private set; }
        public bool     IsCloseable { get; private set; } = true;
        public Widget   Buttons     { get; private set; }
        public string[] Arguments   { get; private set; }

        private AlertDialogWidget() {
        }

        public AlertDialogWidget NonCloseable() {
            this.IsCloseable = false;
            return this;
        }

        public AlertDialogWidget WithButton(Widget buttons) {
            this.Buttons = buttons;
            return this;
        }

        public AlertDialogWidget WithArgs(params string[] args) {
            this.Arguments = args;
            return this;
        }

        public AlertDialogWidget WithTwoButtons(Widget buttonA, Widget buttonB) {
            this.Buttons = new Row {Children = {buttonA, buttonB}};
            return this;
        }

        public static AlertDialogWidget Message(string suffix) => new AlertDialogWidget {
            Title = $"ALERT_TITLE_{suffix}",
            Text  = $"ALERT_MESSAGE_{suffix}",
        };

        public static AlertDialogWidget Ok(string suffix)          => Message(suffix).WithButton(OkBtn);
        public static AlertDialogWidget OkCancel(string suffix)    => Message(suffix).WithTwoButtons(OkBtn, CancelBtn);
        public static AlertDialogWidget Continue(string suffix)    => Message(suffix).WithButton(ContinueBtn);
        public static AlertDialogWidget YesNo(string suffix)       => Message(suffix).WithTwoButtons(YesBtn, NoBtn);
        public static AlertDialogWidget YesCancel(string suffix)   => Message(suffix).WithTwoButtons(YesBtn, CancelBtn);
        public static AlertDialogWidget Retry(string suffix)       => Message(suffix).WithButton(RetryBtn);
        public static AlertDialogWidget RetryCancel(string suffix) => Message(suffix).WithTwoButtons(RetryBtn, CancelBtn);

        private static Widget OkBtn       => new AlertDialogButtonWidget($"ALERT_BUTTON_OK", true);
        private static Widget CancelBtn   => new AlertDialogButtonWidget($"ALERT_BUTTON_CANCEL", false);
        private static Widget YesBtn      => new AlertDialogButtonWidget($"ALERT_BUTTON_YES", true);
        private static Widget NoBtn       => new AlertDialogButtonWidget($"ALERT_BUTTON_NO", false);
        private static Widget ContinueBtn => new AlertDialogButtonWidget($"ALERT_BUTTON_CONTINUE", true);
        private static Widget RetryBtn    => new AlertDialogButtonWidget($"ALERT_BUTTON_RETRY", true);
    }

    public class AlertDialogState : ViewState<AlertDialogWidget>, IAlertDialogState {
        private readonly StateHolder buttonsState;

        public AlertDialogState() {
            this.buttonsState = this.CreateChild(_ => this.Widget.Buttons ?? new Empty());
        }

        public override WidgetViewReference View => UiConstants.Views.Alert.Dialog;

        public string TitleLocalizationKey   => this.Widget.Title;
        public string MessageLocalizationKey => this.Widget.Text;

        public bool   Closeable => this.Widget.IsCloseable;
        public IState Buttons   => this.buttonsState.Value;

        public string GetArgument(int index) {
            return index >= 0 && index < this.Widget.Arguments.Length ? this.Widget.Arguments[index] : string.Empty;
        }

        public void Close() {
            Navigator.Of(this.Context).Pop(false);
        }
    }
}