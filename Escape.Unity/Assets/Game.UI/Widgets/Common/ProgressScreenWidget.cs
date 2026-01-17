namespace Game.UI.Widgets.Common {
    using JetBrains.Annotations;
    using UniMob;
    using UniMob.UI;
    using Views.Common;

    [RequireFieldsInit]
    public class ProgressScreenWidget : StatefulWidget {
        [CanBeNull] public Atom<float>  Progress;
        [CanBeNull] public Atom<string> Message;
        [CanBeNull] public Atom<string> Parameters;
    }

    public class ProgressScreenState : ViewState<ProgressScreenWidget>, IProgressScreenState {
        public override WidgetViewReference View => UiConstants.Views.ProgressScreen;

        public float  Progress    => this.Widget.Progress?.Value ?? 0f;
        public string Message     => this.Widget.Message?.Value ?? string.Empty;
        public string Parameters  => this.Widget.Parameters?.Value ?? string.Empty;
        public bool   AnimateDots => !string.IsNullOrEmpty(this.Widget.Message?.Value);
    }
}