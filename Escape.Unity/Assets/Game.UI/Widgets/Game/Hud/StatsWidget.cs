namespace Game.UI.Widgets.Game.Hud {
    using Services.Photon;
    using Multicast;
    using UniMob.UI;
    using Views.Game.Hud;

    public class StatsWidget : StatefulWidget {
    }

    public class StatsState : ViewState<StatsWidget>, IStatsViewState {
        [Inject] private PhotonService photonService;

        public override WidgetViewReference View => default;

        public int Ping => this.photonService?.Ping ?? 0;
    }
}
