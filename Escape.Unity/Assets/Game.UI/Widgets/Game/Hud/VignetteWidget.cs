namespace Game.UI.Widgets {
    using Domain.Game;
    using Multicast;
    using UniMob.UI;
    using Views;

    public class VignetteWidget : StatefulWidget { }

    public class VignetteState : ViewState<VignetteWidget>, IVignetteViewState {
        [Inject] GameLocalCharacterModel localModel;

        public override WidgetViewReference View => UiConstants.Views.HUD.Vignette;

        public float Health    => localModel.Health;
        public float MaxHealth => localModel.MaxHealth;
    }
}