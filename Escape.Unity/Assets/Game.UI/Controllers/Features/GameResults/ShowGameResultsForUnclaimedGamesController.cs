namespace Game.UI.Controllers.Features.GameResults {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Domain;
    using Multicast;
    using Photon;
    using Services.Photon;
    using Shared.UserProfile.Commands.Game;
    using Shared.UserProfile.Data;
    using Sound;

    [Serializable, RequireFieldsInit]
    public struct ShowGameResultsForUnclaimedGamesControllerArgs : IResultControllerArgs {
        public bool StartLocalSimulation;
    }

    public class ShowGameResultsForUnclaimedGamesController : ResultController<ShowGameResultsForUnclaimedGamesControllerArgs> {
        [Inject] private PhotonService photonService;
        [Inject] private SdUserProfile userProfile;

        protected override async UniTask Execute(Context context) {
            await context.RunChild(new BackgroundAudioLowPassActivationControllerArgs());

            foreach (var playedGame in this.userProfile.PlayedGames.ToList()) {
                if (playedGame.IsPlaying.Value || playedGame.RewardClaimed.Value) {
                    continue;
                }

                await this.ShowResults(context, playedGame);
            }
        }

        private async UniTask ShowResults(Context context, SdGameResult playedGame) {
            await using (await context.RunBgScreenDisposable()) {
                await using (await this.StartSimulation(context, playedGame)) {
                    await context.RunForResult(new ShowGameResultsControllerArgs {
                        GameResult = playedGame,
                    });

                    await using (await context.RunProgressScreenDisposable("claiming_game_rewards", useSystemNavigator: true)) {
                        await context.Server.ExecuteUserProfile(new UserProfileConfirmGameResultCommand {
                            GameId = playedGame.GameId,
                        }, ServerCallRetryStrategy.RetryWithUserDialog);
                    }
                }
            }
        }

        private async UniTask<IUniTaskAsyncDisposable> StartSimulation(Context context, SdGameResult playedGame) {
            if (!this.Args.StartLocalSimulation) {
                return new NoSimulationDisposable();
            }

            var localSimulationArgs = new PhotonLocalSimulationControllerArgs {
                UnitySceneName    = CoreConstants.Scenes.MAIN_MANU_GAME_RESULTS_ADDITIVE,
                GameModeAssetPath = CoreConstants.Quantum.GameModeAssets.MAIN_MENU_GAME_RESULTS,
                Loadout           = playedGame.GameResult.Value.GetLoadout(),
                Storage           = null,
            };

            return await context.RunDisposable(localSimulationArgs);
        }

        private class NoSimulationDisposable : IUniTaskAsyncDisposable {
            public UniTask DisposeAsync() => UniTask.CompletedTask;
        }
    }
}