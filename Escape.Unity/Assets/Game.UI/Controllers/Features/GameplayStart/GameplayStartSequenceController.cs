namespace Game.UI.Controllers.Features.GameplayStart {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.Party;
    using JetBrains.Annotations;
    using Multicast;
    using Photon;
    using Services.Photon;
    using Shared.UserProfile.Commands.Game;
    using Shared.UserProfile.Commands.GameModes;
    using Tutorial;
    using UnityEngine;
    using Widgets.GameModes;

    [Serializable, RequireFieldsInit]
    public struct GameplayStartSequenceControllerArgs : IDisposableControllerArgs {
        public IScenesController ScenesController;
    }

    public class GameplayStartSequenceController : DisposableController<GameplayStartSequenceControllerArgs> {
        [Inject] private PhotonService   photonService;
        [Inject] private TutorialService tutorialService;
        [Inject] private PartyModel partyModel;

        [CanBeNull] private IDisposableController modeSelectorScreen;

        protected override async UniTask Activate(Context context) {
            this.modeSelectorScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.GameModeSelection,
                Page = () => new GameModesMenuWidget {
                    OnGameModeSelect = gameModeKey => this.RequestFlow(this.OnModeSelected, gameModeKey),
                    OnConfirm        = (confirmed, gameModeKey) => this.RequestFlow(this.OnModeConfirmed, (confirmed, gameModeKey)),
                },
            });
            
            await this.tutorialService.On_GameModeSelector_Activated(context);
        }

        private async UniTask OnModeSelected(Context context, string gameModeKey) {
            await this.tutorialService.On_GameModeSelector_ModeSelected(context, gameModeKey);
        }

        private async UniTask OnModeConfirmed(Context context, (bool confirmed, string gameModeKey) args) {
            if (!args.confirmed) {
                await this.modeSelectorScreen.DisposeAsyncNullable();
                await this.DisposeAsync();
                return;
            }

            await this.tutorialService.On_GameModeSelector_GameConfirmed(context, args.gameModeKey);

            await this.LoadGameplay(context, args.gameModeKey);
        }

        private async UniTask LoadGameplay(Context context, string gameModeKey) {
            await this.modeSelectorScreen.DisposeAsyncNullable();

            if (this.photonService.IsConnected) {
                Debug.LogWarning("MM: Disconnecting from Photon before matchmaking");
                await this.photonService.DisconnectAsync(ConnectFailReason.UserRequest);
            }

            Quantum.QuantumReconnectInformation.Reset();

            // Ensure server knows we're no longer in any game
            await context.Server.ExecuteUserProfile(new Shared.UserProfile.Commands.Game.UserProfileLeaveAllGamesCommand(), 
                ServerCallRetryStrategy.RetryWithUserDialog);

            await context.Server.ExecuteUserProfile(new UserProfileSelectGameModeCommand {
                GameModeKey = gameModeKey,
            }, ServerCallRetryStrategy.RetryWithUserDialog);

            Debug.LogWarning($"MM: Join {gameModeKey}");
            var join = await context.Server.MatchmakingJoin(new Game.Shared.DTO.MatchmakingJoinRequest {
                GameModeKey = gameModeKey,
            }, ServerCallRetryStrategy.RetryWithUserDialog);

            if (join.Result == Game.Shared.DTO.EMatchmakingJoinStatus.AlreadyQueued) {
                Debug.LogWarning("MM: Already queued, canceling first");
                await context.Server.MatchmakingCancel(new Game.Shared.DTO.MatchmakingCancelRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
                await UniTask.Delay(TimeSpan.FromMilliseconds(500));
                join = await context.Server.MatchmakingJoin(new Game.Shared.DTO.MatchmakingJoinRequest {
                    GameModeKey = gameModeKey,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            this.partyModel.StartMatchmaking();

            Game.Shared.DTO.MatchmakingStatusResponse status;
            await using (await context.RunSearchGameScreenDisposable(useSystemNavigator: true)) {
                status = await this.WaitForMatchmaking(context);
            }

            this.partyModel.StopMatchmaking();

            if (status == null || status.Join == null) {
                Debug.LogWarning("MM: Timed out waiting for match");
                await this.DisposeAsync();
                return;
            }

            await using (await context.RunLoadingScreenDisposable(useSystemNavigator: true)) {
                await this.Args.ScenesController.GoToEmpty(context);

                var connectionArgs = new PhotonGameConnectArgs {
                    Session    = status.Join.RoomName,
                    Creating   = true,
                    Region     = status.Join.Region,
                };

                Debug.LogWarning($"Photon: Connect Session={connectionArgs.Session} Region={connectionArgs.Region} Creating={connectionArgs.Creating}");
                if (await this.TryConnectToPhoton(context, connectionArgs, status.GameModeKey)) {
                    await context.Server.ExecuteUserProfile(new UserProfileJoinGameCommand {
                        GameId = this.photonService.CurrentGameId,
                    }, ServerCallRetryStrategy.RetryWithUserDialog);

                    await this.Args.ScenesController.GoToGameplay(context);
                }
                else {
                    await this.Args.ScenesController.GoToMainMenu(context);
                }
            }

            await this.DisposeAsync();
        }

        private async UniTask<Game.Shared.DTO.MatchmakingStatusResponse> WaitForMatchmaking(Context context) {
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
            Game.Shared.DTO.MatchmakingStatusResponse status = null;

            while (DateTime.UtcNow < deadline) {
                if (!this.partyModel.IsSearchingMatch.Value) {
                    Debug.LogWarning("MM: Search canceled by user");
                    return null;
                }

                var remaining = (int)(deadline - DateTime.UtcNow).TotalSeconds;
                this.partyModel.UpdateMatchmakingTime(remaining);

                var s = await context.Server.MatchmakingStatus(new Game.Shared.DTO.MatchmakingStatusRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
                if (s != null && s.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched && s.Join != null) {
                    Debug.LogWarning($"MM: Matched Room={s.Join.RoomName} Region={s.Join.Region} Mode={s.GameModeKey}");
                    status = s;
                    break;
                }
                await UniTask.Delay(TimeSpan.FromMilliseconds(250));
            }

            return status;
        }

        private async UniTask<bool> TryConnectToPhoton(Context context, PhotonGameConnectArgs connectionArgs, string gameModeKeyOverride) {
            var connectResult = await context.RunForResult(new PhotonJoinGameControllerArgs {
                connectionArgs      = connectionArgs,
                gameModeKeyOverride = gameModeKeyOverride,
                maxPlayersOverride  = null,
            }, default(ConnectResult));

            return connectResult.Success;
        }
    }
}