namespace Game.UI.Controllers.Features.GameResults {
    using System;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Multicast.Routes;
    using Quantum;
    using Shared.UserProfile.Data;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets;

    [Serializable, RequireFieldsInit]
    public struct ShowGameResultsControllerArgs : IResultControllerArgs {
        public SdGameResult GameResult;
    }

    public class ShowGameResultsController : ResultController<ShowGameResultsControllerArgs> {
        protected override async UniTask Execute(Context context) {
            await this.ShowGameResultsUI(context, this.Args.GameResult);
        }
        
        private async UniTask ShowGameResultsUI(Context context, SdGameResult gameResultsData) {
            var gameResults = gameResultsData.GameResult.Value;

            switch (gameResults) {
                case GameResultsDeathMatch gameResultsDeathMatch:
                    // TODO show game results UI for DM
                    await this.PushGameResultsSimpleAndWait(context, gameResultsData.GameId);
                    break;

                case GameResultsTeamDeathMatch gameResultsTeamDeathMatch:
                    // TODO show game results UI for TDM
                    await this.PushGameResultsSimpleAndWait(context, gameResultsData.GameId);
                    break;

                case GameResultsEscape gameResultsEscape:
                    // TODO show game results UI for Escape
                    await this.PushGameResultsSimpleAndWait(context, gameResultsData.GameId);
                    break;

                default:
                    Debug.LogError($"Unexpected game results: {gameResults?.GetType().Name}");
                    await this.PushGameResultsSimpleAndWait(context, gameResultsData.GameId);
                    break;
            }
        }
        
        private Task PushGameResultsSimpleAndWait(Context context, string playedGameId) {
            return context.RootNavigator.Push(new FadeRoute(
                new RouteSettings("game_results_simple_screen", RouteModalType.Fullscreen),
                (buildContext, animation, secondaryAnimation) => new GameResultsWithInventoryWidget {
                    PlayedGameId = playedGameId,
                    OnClose      = () => context.RootNavigator.Pop(),
                }
            )).PopTask;
        }
    }
}