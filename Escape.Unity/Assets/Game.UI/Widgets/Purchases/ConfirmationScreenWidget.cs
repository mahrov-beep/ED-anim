namespace Game.UI.Widgets.Common {
    using System;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Common;

    public class ConfirmationScreenWidget : StatefulWidget {
        public Action<bool> OnResult { get; set; }
        public Widget       Content  { get; set; }
    }

    public class ConfirmationScreenState : ViewState<ConfirmationScreenWidget>, IConfirmationScreenState {
        public override  WidgetViewReference View => UiConstants.Views.ConfirmationScreen;
        private readonly StateHolder         contentState;

        public ConfirmationScreenState() {
            this.contentState = this.CreateChild(_ => this.Widget.Content ?? new Empty());
        }

        public void Confirm() {
            this.Widget.OnResult?.Invoke(true);
        }

        public void Decline() {
            this.Widget.OnResult?.Invoke(false);
        }

        public IState Content => this.contentState.Value;
    }
}