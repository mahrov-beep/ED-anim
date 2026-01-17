namespace Game.UI.Controllers.Features.Store {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Domain.Models.Purchases;
    using Multicast;
    using Multicast.DropSystem;
    using Multicast.Purchasing;
    using Shared.UserProfile.Commands.Rewards;

    [Serializable, RequireFieldsInit]
    public struct IapPurchaseControllerArgs : IResultControllerArgs {
        public string storeItemKey;
    }

    public class IapPurchaseController : ResultController<IapPurchaseControllerArgs> {
        [Inject] private readonly StoreItemsModel storeItemsModel;
        [Inject] private readonly UdPurchasesRepo purchasesData;

        protected override async UniTask Execute(Context context) {
            var storeItem = this.storeItemsModel.Get(this.Args.storeItemKey);

            var purchaseKey = storeItem.IapPurchaseKey;

            if (string.IsNullOrEmpty(purchaseKey)) {
                throw new InvalidOperationException($"Failed to purchase {this.Args.storeItemKey}: no iap purchase key");
            }

            var extraDrops = storeItem.ExtraDrops
                .Select(it => Drop.FromReward(RewardBuildUtility.Build(it)))
                .ToList();

            await context.RunForResult(new PerformPurchaseControllerArgs {
                PurchaseKey = purchaseKey,
                Drops       = extraDrops,
                ItemKey     = purchaseKey,
            });

            App.RequestAppUpdateFlow();
        }
    }
}