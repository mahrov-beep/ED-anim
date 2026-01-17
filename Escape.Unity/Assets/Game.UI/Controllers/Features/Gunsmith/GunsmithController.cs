namespace Game.UI.Controllers.Features.Gunsmith {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Routes;
    using Shared;
    using Shared.UserProfile.Commands.Features;
    using Shared.UserProfile.Commands.Gunsmiths;
    using Sound;
    using Storage;
    using Tutorial;
    using UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.Gunsmiths;

    [Serializable, RequireFieldsInit]
    public struct GunsmithControllerArgs : IFlowControllerArgs {
    }

    public class GunsmithController : FlowController<GunsmithControllerArgs> {
        [Inject] private TutorialService tutorialService;

        private IUniTaskAsyncDisposable gunsmithScreen;
        private IUniTaskAsyncDisposable bgScreen;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);

            GunsmithFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));
            GunsmithFeatureEvents.BuyLoadout.Listen(this.Lifetime, args => this.RequestFlow(this.BuyLoadout, args));

            await this.tutorialService.On_GunsmithMenu_Activated(context);
        }

        private async UniTask Open(Context context) {
            const string gunsmithKey = SharedConstants.Game.Gunsmiths.GUNSMITH_1;

            this.bgScreen = await context.RunBgScreenDisposable();
            await context.RunChild(new BackgroundAudioLowPassActivationControllerArgs());

            await using (await context.RunProgressScreenDisposable("refreshing_gunsmith")) {
                await context.Server.ExecuteUserProfile(new UserProfileGunsmithRefreshCommand {
                    GunsmithKey = gunsmithKey,
                }, ServerCallRetryStrategy.Throw);

                await context.Server.ExecuteUserProfile(new UserProfileViewFeatureCommand {
                    FeatureKey = SharedConstants.Game.Features.GUNSMITH,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            this.gunsmithScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.GunsmithMenu,
                Page = () => new GunsmithMenuWidget {
                    GunsmithKey = gunsmithKey,
                    OnClose     = () => this.RequestFlow(this.Close),
                },
            });
        }

        private async UniTask Close(Context context) {
            await this.tutorialService.On_GunsmithMenu_Close(context);

            await this.gunsmithScreen.DisposeAsync();
            await this.bgScreen.DisposeAsync();
            this.Stop();
        }

        private async UniTask BuyLoadout(Context context, GunsmithFeatureEvents.BuyLoadoutArgs args) {
            await this.tutorialService.On_GunsmithMenu_LoadoutBuy(context);

            await using (await context.RunFadeScreenDisposable())
            await using (await context.RunProgressScreenDisposable("processing")) {
                await context.Server.ExecuteUserProfile(new UserProfileGunsmithBuyLoadoutCommand {
                    GunsmithKey         = args.gunsmithKey,
                    GunsmithLoadoutGuid = args.gunsmithLoadoutGuid,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            StorageFeatureEvents.Open.Raise();
        }
    }
}