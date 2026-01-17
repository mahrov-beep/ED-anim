namespace Game.UI.Widgets.Tutorial {
    using Controllers.Tutorial;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Tutorial;

    [RequireFieldsInit]
    public class TutorialWidget : StatefulWidget {
        public RouteSettings Route;
    }

    public class TutorialState : ViewState<TutorialWidget>, ITutorialState {
        public override WidgetViewReference View => UiConstants.Views.Tutorial.MaskScreen;

        public bool TutorialEnabled => TutorialStatics.RoutesWithActiveTutorial.Contains(this.Widget.Route);
    }
}