namespace Game.UI.Widgets.Game.Hud {
    using System.Collections.Generic;
    using _Project.Scripts.Minimap;
    using Domain.Game;
    using Multicast;
    using UniMob.UI;
    using Views.Game.Hud;

    public class ListenedCueWidget : StatefulWidget { }

    public class ListenedCueState : ViewState<ListenedCueWidget>, IListenedCueViewState {
        [Inject] ListenedCueModel listenedCueModel;

        public override WidgetViewReference View => UiConstants.Views.HUD.ListenedCue;

        public List<CueData> StepsScreenNormalizedDirections =>
                        listenedCueModel.StepsScreenNormalizedDirections;

        public List<CueData> ShootScreenNormalizedDirections =>
                        listenedCueModel.ShootsScreenNormalizedDirections;
    }
}