namespace Game.UI.Controllers.Features.Thresher {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Routes;
    using Shared.UserProfile.Commands.Thresher;
    using Sound;
    using UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.Threshers;

    [Serializable, RequireFieldsInit]
    public struct ThresherControllerArgs : IFlowControllerArgs {
    }

    public class ThresherController : FlowController<ThresherControllerArgs> {
        private IUniTaskAsyncDisposable threshersScreen;
        private IUniTaskAsyncDisposable bgScreen;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);

            ThresherFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));
            ThresherFeatureEvents.LevelUp.Listen(this.Lifetime, args => this.RequestFlow(this.LevelUp, args));
        }

        private async UniTask Open(Context context) {
            this.bgScreen = await context.RunBgScreenDisposable();
            await context.RunChild(new BackgroundAudioLowPassActivationControllerArgs());
            this.threshersScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.ThreshersMenu,
                Page = () => new ThreshersMenuWidget {
                    OnClose = () => this.RequestFlow(this.Close),
                },
            });
        }

        private async UniTask Close(Context context) {
            await this.threshersScreen.DisposeAsync();
            await this.bgScreen.DisposeAsync();
            this.Stop();
        }

        private async UniTask LevelUp(Context context, ThresherFeatureEvents.LevelUpArgs args) {
            await using (await context.RunFadeScreenDisposable())
            await using (await context.RunProgressScreenDisposable("processing")) {
                await context.Server.ExecuteUserProfile(new UserProfileThresherLevelUpCommand {
                    ThresherKey = args.thresherKey,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }
        }
    }
}