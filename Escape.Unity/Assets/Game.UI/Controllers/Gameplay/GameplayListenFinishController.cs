namespace Game.UI.Controllers.Gameplay {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using Shared.UserProfile.Data;

    [Serializable, RequireFieldsInit]
    public struct GameplayListenFinishControllerArgs : IFlowControllerArgs {
        public IScenesController ScenesController;
    }

    public class GameplayListenFinishController : FlowController<GameplayListenFinishControllerArgs> {
        [Inject] private PhotonService photonService;
        [Inject] private SdUserProfile userProfile;

        private GameSnapshot toFinishGameSnapshot;

        protected override async UniTask Activate(Context context) {
            this.Lifetime.Register(QuantumEvent.SubscribeManual<EventGameExit>(evt => {
                this.toFinishGameSnapshot = evt.GameSnapshot;
                App.RequestAppUpdateFlow();
            }));

            this.Lifetime.Register(QuantumEvent.SubscribeManual<EventExitZoneUsed>(evt => {
                var player = this.photonService.GetPlayerByActorId(evt.PhotonActorId);

                if (!player.IsLocal) {
                    return;
                }

                this.toFinishGameSnapshot = evt.GameSnapshot;
                App.RequestAppUpdateFlow();
            }));

            this.Lifetime.Register(QuantumEvent.SubscribeManual<EventGameLost>(evt => {
                var player = this.photonService.GetPlayerByActorId(evt.PhotonActorId);

                if (!player.IsLocal) {
                    return;
                }

                this.toFinishGameSnapshot = evt.GameSnapshot;
                App.RequestAppUpdateFlow();
            }));
        }

        protected override async UniTask OnFlow(Context context) {
            if (this.toFinishGameSnapshot is { } gameSnapshot) {
                this.toFinishGameSnapshot = null;
                await context.RunForResult(new GameplayFinishControllerArgs {
                    ScenesController = this.Args.ScenesController,
                    gameSnapshot     = gameSnapshot,
                });
            }
        }
    }
}