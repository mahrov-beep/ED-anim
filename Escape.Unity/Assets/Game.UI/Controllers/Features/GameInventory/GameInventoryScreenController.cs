namespace Game.UI.Controllers.Features.GameInventory {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Routes;
    using SelectedItemInfo;
    using Sound;
    using UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets;

    [Serializable, RequireFieldsInit]
    public struct GameInventoryScreenControllerArgs : IFlowControllerArgs {
    }

    public class GameInventoryScreenController : FlowController<GameInventoryScreenControllerArgs> {
        private IUniTaskAsyncDisposable screenController;

        private IUniTaskAsyncDisposable selectedItemInfoController;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);

            this.selectedItemInfoController = await context.RunDisposable(new SelectedItemInfoFeatureControllerArgs());

            GameInventoryFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));
        }

        private async UniTask Open(Context context) {
            await context.RunChild(new GameInventoryControlsControllerArgs());
            await context.RunChild(new BackgroundAudioLowPassActivationControllerArgs());

            this.screenController = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.GameInventory,
                Page  = null,
                PageAnimated = (buildContext, animation, secondaryAnimation) => new StatsWithInventoryWidget {
                    OnClose   = () => GameInventoryFeatureEvents.Close.Raise(),
                    Animation = animation,
                },
                TransitionDuration        = 0.1f,
                ReverseTransitionDuration = 0.05f,

                OnBackPerformed = () => GameInventoryFeatureEvents.Close.Raise(),
            });
        }

        private async UniTask Close(Context context) {
            await this.selectedItemInfoController.DisposeAsync();
            await this.screenController.DisposeAsync();
            this.Stop();
        }
    }
}