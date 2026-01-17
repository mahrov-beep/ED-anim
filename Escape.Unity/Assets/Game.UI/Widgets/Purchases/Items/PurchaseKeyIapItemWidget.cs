namespace Game.UI.Widgets.Purchases.Items {
    using System;
    using System.Linq;
    using Controllers.Features.Store;
    using Domain.Currencies;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Numerics;
    using Multicast.Purchasing;
    using Scellecs.Morpeh;
    using Shared;
    using Shared.Defs;
    using UniMob.UI;
    using Views.Purchases.Items;

    public class PurchaseKeyIapItemWidget : StatefulWidget {
        public string PurchaseKey  { get; }
        public string CategoryKey  { get; }
        public string StoreItemKey { get; }

        public WidgetViewReference ViewReference { get; set; } = UiConstants.Views.Purchases.Items.KeyIap;

        public PurchaseKeyIapItemWidget([NotNull] string purchaseKey, [NotNull] string categoryKey, [NotNull] string storeItemKey) {
            this.PurchaseKey  = purchaseKey ?? throw new ArgumentNullException(nameof(purchaseKey));
            this.CategoryKey  = categoryKey ?? throw new ArgumentNullException(nameof(categoryKey));
            this.StoreItemKey = storeItemKey ?? throw new ArgumentNullException(nameof(storeItemKey));
        }
    }

    public class PurchaseKeyIapItemState : ViewState<PurchaseKeyIapItemWidget>, IPurchaseKeyIapItemState {
        public override WidgetViewReference View => this.Widget.ViewReference;

        public string PurchaseKey    => this.Widget.PurchaseKey;
        public string LocalizedPrice => this.purchasing.GetLocalizedPriceString(this.PurchaseKey);

        public BigDouble Amount => this.ParseAmount();

        public bool IsSold   => this.CheckIfSold();
        public bool IsFinite => false;

        private readonly GameDef gameDef;

        private readonly IPurchasing     purchasing;
        private readonly World           world;
        private readonly CurrenciesModel currencies;

        private StoreItemDef      StoreItemDef      => this.gameDef.StoreItems.Get(this.Widget.StoreItemKey);
        private PurchaseDef       PurchaseDef       => this.gameDef.Purchases.Get(this.PurchaseKey);
        private CurrencyRewardDef CurrencyRewardDef => this.StoreItemDef.extraDrops.First() as CurrencyRewardDef;


        public PurchaseKeyIapItemState(GameDef gameDef, IPurchasing purchasing, World world, CurrenciesModel currencies) {
            this.gameDef    = gameDef;
            this.purchasing = purchasing;
            this.world      = world;
            this.currencies = currencies;
        }

        public void Purchase() {
            if (!this.CheckIfSold()) {
                StoreFeatureEvents.Purchase.Raise(new StoreFeatureEvents.PurchaseArgs {
                    storeItemKey = this.Widget.StoreItemKey,
                });
            }
        }


        private bool CheckIfSold() {
            return this.HasCurrencyDrop(out var currencyModel);
        }

        private bool HasCurrencyDrop(out CurrencyModel currencyModel) {
            currencyModel = null;
            return this.CurrencyRewardDef != null && this.currencies.TryGet(this.CurrencyRewardDef.currency, out currencyModel);
        }

        private BigDouble ParseAmount() {
            if (this.CurrencyRewardDef != null) {
                return this.CurrencyRewardDef.amount;
            }

            return BigDouble.Zero;
        }
    }
}