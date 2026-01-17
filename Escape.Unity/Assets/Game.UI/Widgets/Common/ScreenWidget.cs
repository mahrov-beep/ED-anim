namespace Game.UI.Widgets.Common {
    using System;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Common;

    public class ScreenWidget : StatefulWidget {
        public const float TOP_PADDING = 100;

        public Widget Header  { get; set; }
        public Widget Content { get; set; }
        public Action OnClose { get; set; }
    }

    public class ScreenState : ViewState<ScreenWidget>, IScreenState {
        public override WidgetViewReference View => UiConstants.Views.ScreenView;

        public IState Content => this.contentState.Value;
        public IState Header  => this.headerState.Value;
        
        private readonly StateHolder contentState;
        private readonly StateHolder headerState;

        public ScreenState() {
            this.contentState = this.CreateChild(_ => this.Widget.Content ?? new Empty());
            this.headerState = this.CreateChild(_ => this.Widget.Header ?? new Empty());
        }

        public void OnClose() {
            this.Widget.OnClose?.Invoke();
        }
    }
}