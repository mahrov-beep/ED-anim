namespace Game.UI.Controllers.Features.ExpProgressionRewards {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.ExpProgressionRewards;
    using Multicast;
    using Sound;
    using UniMob.UI;
    using Widgets.ExpProgressionRewards;

    [Serializable, RequireFieldsInit]
    public struct ExpProgressionRewardsScreenControllerArgs : IFlowControllerArgs {
    }

    public class ExpProgressionRewardsScreenController : FlowController<ExpProgressionRewardsScreenControllerArgs> {
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;

        private IUniTaskAsyncDisposable rewardsScreen;
        private IUniTaskAsyncDisposable bgScreen;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);

            ExpProgressionRewardsFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));
        }

        private async UniTask Open(Context context) {
            this.bgScreen = await context.RunBgScreenDisposable();
            await context.RunChild(new BackgroundAudioLowPassActivationControllerArgs());

            await context.RunForResult(new ExpProgressionRewardsLevelUpControllerArgs());

            var rewardsScreenKey = new GlobalKey<ExpProgressionRewardsScreenState>();

            this.rewardsScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.ExpProgressionScreen,
                Page = () => new ExpProgressionRewardsScreenWidget {
                    Key     = rewardsScreenKey,
                    OnClose = () => this.RequestFlow(this.Close),
                },
            });

            await UniTask.WaitUntil(() => rewardsScreenKey.CurrentState is { IsScrollMounted: true });
            rewardsScreenKey.CurrentState.ScrollToSelected();
        }

        private async UniTask Close(Context context) {
            await this.rewardsScreen.DisposeAsync();
            await this.bgScreen.DisposeAsync();
            this.expProgressionRewardsModel.Selected = null;
            this.Stop();
        }
    }
}