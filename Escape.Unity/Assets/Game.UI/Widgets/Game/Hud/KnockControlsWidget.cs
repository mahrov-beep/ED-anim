namespace Game.UI.Widgets.Game.Hud {
    using Domain.Game;
    using Multicast;
    using UniMob.UI;
    using Views.Game.Hud;

    [RequireFieldsInit]
    public sealed class KnockControlsWidget : StatefulWidget {
    }

    public sealed class KnockControlsState : ViewState<KnockControlsWidget>, IKnockControlsViewState {
        [Inject] private GameLocalCharacterModel localCharacterModel;

        public override WidgetViewReference View => default;

        public bool HideInput => this.localCharacterModel?.IsKnocked ?? false;

        public override WidgetSize CalculateSize() {
            return WidgetSize.Stretched;
        }
    }
}
