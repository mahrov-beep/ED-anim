namespace Game.UI.Widgets.Game.Hud {
    using System.Collections.Generic;
    using Domain.Game;
    using Multicast;
    using UniMob.UI;
    using Views.Game.Hud;
    public class DamageSourceCueWidget : StatefulWidget { }
    
    
    public class DamageSourceCueState : ViewState<DamageSourceCueWidget>, IDamageSourceCueViewState {
        [Inject] private DamageSourceModel damageSourceModel;

        public override WidgetViewReference View => UiConstants.Views.HUD.DamageCue;

        public List<DamageSourceData> DamageSources => damageSourceModel.DamageSourceScreenNormalizedDirections;
    }
}