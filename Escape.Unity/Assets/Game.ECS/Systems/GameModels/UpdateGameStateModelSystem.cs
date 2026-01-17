namespace Game.ECS.Systems.GameModels {
    using Domain.Game;
    using Multicast;
    using Scellecs.Morpeh;
    using Services.Photon;

    public unsafe class UpdateGameStateModelSystem : SystemBase {
        [Inject] private PhotonService  photonService;
        [Inject] private GameStateModel gameStateModel;

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            this.gameStateModel.GameState         = f.Global->GameState;
            this.gameStateModel.SecondsToStateEnd = f.Global->GameStateTimer.AsInt;
        }
    }
}