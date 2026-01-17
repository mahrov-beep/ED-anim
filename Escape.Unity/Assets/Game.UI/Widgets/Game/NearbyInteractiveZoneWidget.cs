namespace Game.UI.Widgets.Game {
    using Domain.GameInventory;
    using ECS.Systems.Player;
    using Multicast;
    using Quantum;
    using Quantum.InteractiveZones;
    using Services.Photon;
    using UniMob.UI;
    using Views.Game;

    [RequireFieldsInit]
    public class NearbyInteractiveZoneWidget : StatefulWidget {
    }

    public class NearbyInteractiveZoneState : ViewState<NearbyInteractiveZoneWidget>, INearbyInteractiveZoneState {
        [Inject] private PhotonService                  photonService;
        [Inject] private LocalPlayerSystem              localPlayerSystem;
        [Inject] private GameNearbyInteractiveZoneModel interactiveZoneModel;
        [Inject] private ITimeService                   timeService;

        private EntityRef            ZoneEntity => this.interactiveZoneModel.NearbyInteractiveZone;
        private InteractiveZone      Zone       => this.photonService.PredictedFrame!.Get<InteractiveZone>(this.ZoneEntity);
        private InteractiveZoneAsset ZoneAsset  => this.photonService.PredictedFrame!.FindAsset(this.Zone.Asset);

        public override WidgetViewReference View => this.ZoneAsset switch {
            ExitInteractiveZoneAsset => UiConstants.Views.Game.NearbyInteractiveZoneExit,
            _ => default,
        };

        public float RemainingTime {
            get {
                var _ = this.timeService.Now;
                if (!this.photonService.TryGetPredicted(out var f)) {
                    return 0f;
                }

                if (this.localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                    return 0f;
                }

                if (!f.TryGet(localRef, out Unit unit)) {
                    return 0f;
                }

                return unit.ExitZoneTimer.TimeLeft.AsFloat;
            }
        }

        public float TotalTime => this.ZoneAsset.interactionTime.AsFloat;
    }
}
