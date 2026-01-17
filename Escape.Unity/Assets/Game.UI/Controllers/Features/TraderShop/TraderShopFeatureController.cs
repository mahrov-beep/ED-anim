namespace Game.UI.Controllers.Features.TraderShop {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct TraderShopFeatureControllerArgs : IFlowControllerArgs {
    }

    public class TraderShopFeatureController : FlowController<TraderShopFeatureControllerArgs> {
        [CanBeNull] private IControllerBase traderShopController;

        protected override async UniTask Activate(Context context) {
            TraderShopFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenTraderShop));
        }

        private async UniTask OpenTraderShop(Context context) {
            if (this.traderShopController is { IsRunning: true }) {
                Debug.LogError($"[{this}] TraderShop already opened");
                return;
            }

            this.traderShopController = await context.RunChild(new TraderShopControllerArgs());
        }

        [Button]
        private void RaiseOpenTraderShop() {
            TraderShopFeatureEvents.Open.Raise();
        }

        [Button]
        private void RaiseCloseTraderShop() {
            TraderShopFeatureEvents.Close.Raise();
        }
    }
}