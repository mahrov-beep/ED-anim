namespace Game.UI.Widgets.Game.Hud {
    using Domain.Game;
    using Multicast;
    using UniMob.UI;
    using Views.Game.Hud;

    [RequireFieldsInit]
    public class HealthWidget : StatefulWidget {
    }

    public class HealthState : ViewState<HealthWidget>, IHealthState {
        [Inject] private GameLocalCharacterModel localCharacterModel;

        public override WidgetViewReference View => UiConstants.Views.HUD.Health;

        public float Health             => this.localCharacterModel.Health;
        public float MaxHealth          => this.localCharacterModel.MaxHealth;
        public float KnockHealth        => this.localCharacterModel.KnockHealth;
        public bool  IsKnocked          => this.localCharacterModel.IsKnocked;
        public bool  IsBeingRevived     => this.localCharacterModel.IsBeingRevived;
        public float KnockTimeRemaining => this.localCharacterModel.KnockTimeRemaining;
        public float KnockTimeTotal     => this.localCharacterModel.KnockTimeTotal;
        public float ReviveProgress     => this.localCharacterModel.ReviveProgress;
    }
}