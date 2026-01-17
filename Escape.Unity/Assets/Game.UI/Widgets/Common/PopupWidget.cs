namespace Game.UI.Widgets.Common {
    using System;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Common;

    public class PopupWidget : StatefulWidget {
        public const float DEFAULT_HEIGHT = 930;

        public string PopupKey { get; }

        public PopupWidget(string popupKey) {
            this.PopupKey = popupKey;
        }


        public Widget Content { get; set; }
        public Action OnClose { get; set; }

        public WidgetViewReference ViewReference { get; set; } = UiConstants.Views.Popup;
    }

    public class PopupState : ViewState<PopupWidget>, IPopupState {
        private readonly StateHolder contentState;

        public PopupState() {
            this.contentState = this.CreateChild(_ => this.Widget.Content ?? new Empty());
        }

        public override WidgetViewReference View => this.Widget.ViewReference;

        public string PopupKey => this.Widget.PopupKey;
        public IState Content  => this.contentState.Value;

        public float ContentHeight {
            get {
                var h = this.contentState.Value.Size.MaxHeight;
                return float.IsInfinity(h) ? PopupWidget.DEFAULT_HEIGHT : h;
            }
        }

        public void OnClose() {
            this.Widget.OnClose?.Invoke();
        }
    }
}