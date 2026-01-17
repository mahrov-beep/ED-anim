namespace Game.UI.Controllers.Features.GameResults {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;

    [Serializable, RequireFieldsInit]
    public struct GameResultsFeatureControllerArgs : IFlowControllerArgs {
    }

    public class GameResultsFeatureController : FlowController<GameResultsFeatureControllerArgs> {
        protected override async UniTask Activate(Context context) {
            this.RequestFlow(this.ShowGameResults);
        }

        private async UniTask ShowGameResults(Context context) {
            await context.RunForResult(new ShowGameResultsForUnclaimedGamesControllerArgs {
                StartLocalSimulation = true,
            });
        }
    }
}