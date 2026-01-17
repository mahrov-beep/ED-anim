namespace Game.UI.Widgets.Game {
    using Controllers.Features.GameInventory;
    using Domain.GameInventory;
    using ECS.Systems.Player;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using UniMob;
    using UniMob.UI;
    using Views.Game;

    [RequireFieldsInit]
    public class GameNearbyItemBoxWidget : StatefulWidget {
        public GameNearbyItemBoxModel ItemBoxModel { get; set; }
    }

    public class GameNearbyItemBoxState : ViewState<GameNearbyItemBoxWidget>, IGameNearbyItemBoxState {
        [Inject] private PhotonService        photonService;
        [Inject] private GameNearbyItemsModel gameNearbyItemsModel;
        [Inject] private LocalPlayerSystem    localPlayerSystem;

        public override WidgetViewReference View => this.IsBackpack
            ? UiConstants.Views.Game.NearbyItemBoxBackpack
            : UiConstants.Views.Game.NearbyItemBox;

        private GameNearbyItemBoxModel Model => this.Widget.ItemBoxModel;

        public bool CanEquipBest => this.Model.CanEquipBest;
        public bool CanOpen      => this.Model.CanOpen;

        public bool IsBackpack => this.Model.IsBackpack;

        public bool HasTimer => !this.Model.IsBackpack && this.Model.ItemBoxTime > 0 && this.TimeLeft > 0;

        public int TimeLeft => (int)this.Model.ItemBoxTimer;

        public float Progress => 1f - this.Model.ItemBoxTimer / this.Model.ItemBoxTime;
    }
}