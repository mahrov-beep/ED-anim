namespace Game.UI.Widgets.Gunsmiths {
    using UniMob.UI;
    using Views.Gunsmiths;

    [RequireFieldsInit]
    public class GunsmithLoadoutLevelBlockerWidget : StatefulWidget {
        public string ThresherKey;
        public int    RequiredThresherLevel;
    }

    public class GunsmithLoadoutLevelBlockerState : ViewState<GunsmithLoadoutLevelBlockerWidget>, IGunsmithLoadoutLevelBlockerState {
        public override WidgetViewReference View => UiConstants.Views.Gunsmiths.LevelBlock;

        public string ThresherKey => this.Widget.ThresherKey;

        public int RequiredThresherLevel => this.Widget.RequiredThresherLevel;
    }
}