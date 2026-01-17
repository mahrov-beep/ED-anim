namespace Game.UI.Controllers.Features.Store {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.Models.Purchases;
    using Multicast;
    using Shared.Defs;

    [Serializable, RequireFieldsInit]
    public struct StoreFeatureControllerArgs : IFlowControllerArgs {
    }

    public class StoreFeatureController : FlowController<StoreFeatureControllerArgs> {
        [Inject] private StoreItemsModel storeItemsModel;

        protected override async UniTask Activate(Context context) {
            StoreFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenStore));
            StoreFeatureEvents.Purchase.Listen(this.Lifetime, args => this.RequestFlow(this.Purchase, args));
        }

        private async UniTask OpenStore(Context context) {
            await context.RunChild(new StoreControllerArgs());
        }

        private async UniTask Purchase(Context context, StoreFeatureEvents.PurchaseArgs args) {
            var storeItem = this.storeItemsModel.Get(args.storeItemKey);

            switch (storeItem.ItemType) {
                case StoreItemType.CurrencyPurchase:
                    await context.RunForResult(new CurrencyPurchaseControllerArgs {
                        storeItemKey = args.storeItemKey,
                    });
                    break;

                default:
                    await context.RunForResult(new IapPurchaseControllerArgs {
                        storeItemKey = args.storeItemKey,
                    });
                    break;
            }
        }
    }
}