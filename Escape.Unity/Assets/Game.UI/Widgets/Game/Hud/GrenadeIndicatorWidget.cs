namespace Game.UI.Widgets.Game.Hud {
    using System.Collections.Generic;
    using Domain.Game;
    using Multicast;
    using UniMob.UI;
    using Views.Game.Hud;

    public class GrenadeIndicatorWidget : StatefulWidget { }

    public class GrenadeIndicatorState : ViewState<GrenadeIndicatorWidget>, IGrenadeIndicatorViewState {
        [Inject] GrenadeIndicatorModel grenadeIndicatorModel;

        public override WidgetViewReference View => UiConstants.Views.HUD.GrenadeIndicator;

        public List<GrenadeIndicatorData> GrenadeIndicators => grenadeIndicatorModel.GrenadeIndicators;
    }
}

