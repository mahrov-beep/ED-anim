namespace Game.UI.Widgets.Tutorial {
    using System;
    using UniMob.UI;
    using Views.Tutorial;

    [RequireFieldsInit]
    public class TutorialPopupWidget : StatefulWidget {
        public string TutorialKey;
        public string TutorialStep;

        public Action OnClose;
    }

    public class TutorialPopupState : ViewState<TutorialPopupWidget>, ITutorialPopupState {
        public override WidgetViewReference View => UiConstants.Views.Tutorial.PopupScreen;

        public string TutorialKey => this.Widget.TutorialKey;

        public string TutorialStep => this.Widget.TutorialStep;

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }
    }
}