namespace Game.UI.Controllers.Features.Store {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Domain.AnalyticEvents;
    using Domain.Currencies;
    using Domain.Models.Purchases;
    using Multicast;
    using Multicast.Analytics;
    using Multicast.Numerics;
    using Multicast.Routes;
    using Shared.UserProfile.Commands.Rewards;
    using UniMob.UI.Widgets;
    using Widgets.Purchases;

    [Serializable, RequireFieldsInit]
    public struct CurrencyPurchaseControllerArgs : IResultControllerArgs {
        public string storeItemKey;
    }

    public class CurrencyPurchaseController : ResultController<CurrencyPurchaseControllerArgs> {
        [Inject] private readonly CurrenciesModel        currenciesModel;
        [Inject] private readonly StoreItemsModel        storeItemsModel;
        [Inject] private readonly CurrencyPurchasesModel currencyPurchasesModel;
        [Inject] private readonly IAnalytics             analytics;

        protected override async UniTask Execute(Context context) {
            var storeItemModel     = this.storeItemsModel.Get(this.Args.storeItemKey);
            
            if (string.IsNullOrEmpty(storeItemModel.CurrencyPurchaseKey)) {
                throw new InvalidOperationException($"Failed to purchase {this.Args.storeItemKey}: no currency purchase key");
            }

            var purchaseModel      = this.currencyPurchasesModel.Get(storeItemModel.CurrencyPurchaseKey);
            var priceCurrencyModel = this.currenciesModel.Get(purchaseModel.PriceCurrency);

            if (!priceCurrencyModel.HasEnough(purchaseModel.Price)) {
                await context.RootNavigator.Push(new SlideDownOverlayRoute(
                    UiConstants.Views.BlackOverlay,
                    new RouteSettings("purchase_not_enough", RouteModalType.Popup),
                    (buildContext, animation, secondaryAnimation) => new PurchaseNotEnoughWidget() {
                        OnClose = () => context.RootNavigator.Pop(),
                    }
                )).WithPopOnBack(context.RootNavigator).PushTask;
                return;
            }

            var confirmedByUser = await context.RunForResult(new PurchaseConfirmationPromptControllerArgs {
                storeItemKey = storeItemModel.Key,
            }, default(bool));

            if (!confirmedByUser) {
                return;
            }

            var currency = this.currenciesModel.Get(purchaseModel.PriceCurrency);

            if (!currency.HasEnough(purchaseModel.Price)) {
                throw new InvalidOperationException("No currency");
            }

            if (purchaseModel.Reward == BigDouble.Zero) {
                throw new InvalidOperationException("Has not reward");
            }

            await using (await context.RunFadeScreenDisposable())
            await using (await context.RunProgressScreenDisposable("buying")) {
                await context.Server.ExecuteUserProfile(new PurchaseCurrencyCommand {
                    CurrenciesToBuy = new Dictionary<string, int> {
                        [purchaseModel.RewardCurrency] = purchaseModel.Reward.RoundToIntUnsafe(),
                    },
                    CurrenciesToTake = new Dictionary<string, int> {
                        [purchaseModel.PriceCurrency] = purchaseModel.Price.RoundToIntUnsafe(),
                    },
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            this.analytics.Send(new SpentHardCurrencyAnalyticsEvent(purchaseModel.Key, storeItemModel.Category, currency.Key, (long)purchaseModel.Reward.ToDoubleUnsafe()));
        }
    }
}