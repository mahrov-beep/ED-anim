namespace Game.UI.Controllers.Features.Store {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Routes;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.Storage;

    [Serializable, RequireFieldsInit]
    public struct StoreControllerArgs : IFlowControllerArgs {
    }

    public class StoreController : FlowController<StoreControllerArgs> {
        private IUniTaskAsyncDisposable storeScreen;
        private IUniTaskAsyncDisposable bgScreen;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);

            StoreFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));
        }

        private async UniTask Open(Context context) {
            this.bgScreen = await context.RunBgScreenDisposable();
            this.storeScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.Store,
                Page = () => new StoreWidget {
                    OnClose = () => StoreFeatureEvents.Close.Raise(),
                },
            });
        }

        private async UniTask Close(Context context) {
            await this.storeScreen.DisposeAsync();
            await this.bgScreen.DisposeAsync();
            this.Stop();
        }
    }
}