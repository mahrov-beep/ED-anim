namespace Game.UI.Controllers.Photon {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Domain.GameModes;
    using Multicast;
    using Multicast.GameProperties;
    using Quantum;
    using Services.Photon;
    using Shared.UserProfile.Commands.Game;
    using Shared.UserProfile.Data;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct ReconnectOnStartupControllerArgs : IResultControllerArgs<bool> {
        public ScenesController ScenesController;
    }

    public class ReconnectOnStartupController : ResultController<ReconnectOnStartupControllerArgs, bool> {
#if UNITY_EDITOR
        private static readonly string SkipMenuEditorPrefsKey = $"GameProperty.{GameProperties.Booleans.SkipMainMenu.Name}";
#endif

        [Inject] private PhotonService       photonService;
        [Inject] private GamePropertiesModel gameProperties;
        [Inject] private SdUserProfile       userProfile;
        [Inject] private GameModesModel      gameModesModel;

        protected override async UniTask<bool> Execute(Context context) {
#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetBool(SkipMenuEditorPrefsKey, false)) {
                Debug.Log($"[{nameof(ReconnectOnStartupController)}]: Skip main menu enabled, starting new game");

                await using (await context.RunLoadingScreenDisposable(useSystemNavigator: true)) {
                    await this.Args.ScenesController.GoToEmpty(context);

                    if (await this.TryStartNewGame(context)) {
                        await this.Args.ScenesController.GoToGameplay(context);
                        return true;
                    }

                    return false;
                }
            }
#endif

            var reconnectInformation = QuantumReconnectInformation.Load();

            if (reconnectInformation.HasTimedOut) {
                Debug.Log($"[{nameof(ReconnectOnStartupController)}]: Reconnect not possible: {reconnectInformation}");

                await this.LeaveAllGames(context);
                return false;
            }

            if (this.gameProperties.Get(GameProperties.Booleans.DisableReconnect)) {
                Debug.Log($"[{nameof(ReconnectOnStartupController)}]: Reconnect disabled in cheats");

                await this.LeaveAllGames(context);
                return false;
            }

            await using (await context.RunLoadingScreenDisposable(useSystemNavigator: true)) {
                await this.Args.ScenesController.GoToEmpty(context);

                if (await this.TryReconnect(context)) {
                    await this.Args.ScenesController.GoToGameplay(context);
                    return true;
                }

                await this.LeaveAllGames(context);
                return false;
            }
        }

        private async UniTask LeaveAllGames(Context context) {
            if (this.userProfile.PlayedGames.All(it => it.IsPlaying.Value == false)) {
                return;
            }

            Debug.Log($"[{nameof(ReconnectOnStartupController)}]: Force exit all playing games on meta server");

            await context.Server.ExecuteUserProfile(new UserProfileLeaveAllGamesCommand(), ServerCallRetryStrategy.RetryWithUserDialog);
        }

        private async UniTask<bool> TryReconnect(Context context) {
            Debug.Log($"[{nameof(ReconnectOnStartupController)}]: Found valid reconnect information, trying to reconnect");

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

        private async UniTask<bool> TryStartNewGame(Context context) {
            var connectionArgs = new PhotonGameConnectArgs {
                Session      = null,
                Creating     = true,
                Reconnecting = false,
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