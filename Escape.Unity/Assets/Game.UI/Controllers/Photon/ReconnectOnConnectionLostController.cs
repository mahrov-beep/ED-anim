namespace Game.UI.Controllers.Photon {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.GameModes;
    using Multicast;
    using Quantum;
    using Services.Photon;

    [Serializable, RequireFieldsInit]
    public struct ReconnectOnConnectionLostControllerArgs : IFlowControllerArgs {
        public IScenesController ScenesController;
    }

    public class ReconnectOnConnectionLostController : FlowController<ReconnectOnConnectionLostControllerArgs> {
        [Inject] private PhotonService photonService;
        [Inject] private GameModesModel gameModesModel;

        private string toProcessDisconnectReason;

        protected override async UniTask Activate(Context context) {
            this.Lifetime.Bracket(
                handler => this.photonService.UnexpectedlyDisconnected += handler,
                handler => this.photonService.UnexpectedlyDisconnected -= handler,
                new Action<string>(reason => {
                    this.toProcessDisconnectReason = reason;
                    App.RequestAppUpdateFlow();
                }));
        }

        protected override async UniTask OnFlow(Context context) {
            if (this.toProcessDisconnectReason == null) {
                return;
            }

            this.toProcessDisconnectReason = null;

            await using (await context.RunLoadingScreenDisposable(useSystemNavigator: true)) {
                await this.Args.ScenesController.GoToEmpty(context);

                if (await this.TryReconnect(context)) {
                    await this.Args.ScenesController.GoToGameplay(context);
                }
                else {
                    await this.Args.ScenesController.GoToMainMenu(context);
                }
            }

            this.toProcessDisconnectReason = null;
        }

        private async UniTask<bool> TryReconnect(Context context) {
            var reconnectInformation = QuantumReconnectInformation.Load();

            await using (await context.RunProgressScreenDisposable("disconnecting", useSystemNavigator: true)) {
                await this.photonService.DisconnectAsync(ConnectFailReason.Disconnect);
            }

            if (reconnectInformation.HasTimedOut) {
                return false;
            }

            var connectionArgs = new PhotonGameConnectArgs {
                Session      = null,
                Creating     = false,
                Reconnecting = true,
            };

            var connectResult = await context.RunForResult(new PhotonJoinGameControllerArgs {
                connectionArgs      = connectionArgs,
                gameModeKeyOverride = this.gameModesModel.SelectedGameMode.Def.key,
                maxPlayersOverride  = this.gameModesModel.SelectedGameMode.ModeQuantumAsset.maxPlayers,
            }, default(ConnectResult));

            return connectResult.Success;
        }
    }
}