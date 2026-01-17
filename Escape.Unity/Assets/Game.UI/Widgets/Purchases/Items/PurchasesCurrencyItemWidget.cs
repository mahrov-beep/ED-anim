namespace Game.UI.Widgets.Purchases.Items {
    using System;
    using Controllers.Features.Store;
    using Domain.Currencies;
    using Domain.Models.Purchases;
    using JetBrains.Annotations;
    using Multicast.Numerics;
    using UniMob.UI;
    using Views.Purchases.Items;

    public class PurchasesCurrencyItemWidget : StatefulWidget {
        private readonly Action onBuy;
        public           string CurrencyPurchaseKey { get; }
        public           string CategoryKey         { get; }
        public           string StoreItemKey        { get; }

        public WidgetViewReference ViewReference { get; set; } = UiConstants.Views.Purchases.Items.Currency;

        public PurchasesCurrencyItemWidget([NotNull] string currencyPurchaseKey, [NotNull] string categoryKey, [NotNull] string storeItemKey) {
            this.CurrencyPurchaseKey = currencyPurchaseKey ?? throw new ArgumentNullException(nameof(currencyPurchaseKey));
            this.CategoryKey         = categoryKey ?? throw new ArgumentNullException(nameof(categoryKey));
            this.StoreItemKey        = storeItemKey ?? throw new ArgumentNullException(nameof(storeItemKey));
        }
    }

    public class PurchasesCurrencyItemState : ViewState<PurchasesCurrencyItemWidget>, IPurchasesCurrencyItemState {
        public override WidgetViewReference View => this.Widget.ViewReference;

        public string    CategoryKey    => this.Widget.CategoryKey;
        public string    PurchaseKey    => this.Widget.CurrencyPurchaseKey;
        public string    PriceCurrency  => this.CurrencyPurchase.PriceCurrency;
        public string    RewardCurrency => this.CurrencyPurchase.RewardCurrency;
        public BigDouble Price          => this.CurrencyPurchase.Price;
        public BigDouble Reward         => this.CurrencyPurchase.Reward;

        private CurrencyModel         PriceCurrencyModel => this.currenciesModel.Get(this.PriceCurrency);
        private CurrencyPurchaseModel CurrencyPurchase   => this.currencyPurchasesModel.Get(this.PurchaseKey);


        private readonly StateHolder            dropStateHolder;
        private readonly CurrenciesModel        currenciesModel;
        private readonly CurrencyPurchasesModel currencyPurchasesModel;

        public PurchasesCurrencyItemState(CurrenciesModel currenciesModel, CurrencyPurchasesModel currencyPurchasesModel) {
            this.currenciesModel        = currenciesModel;
            this.currencyPurchasesModel = currencyPurchasesModel;
        }

        public void StartPurchase() {
            StoreFeatureEvents.Purchase.Raise(new StoreFeatureEvents.PurchaseArgs {
                storeItemKey = this.Widget.StoreItemKey,
            });
        }
    }
}