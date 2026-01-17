namespace Game.UI.Widgets.Purchases.Items {
    using System;
    using System.Linq;
    using Controllers.Features.Store;
    using Domain.Models.Purchases;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Purchasing;
    using Scellecs.Morpeh;
    using Shared;
    using UniMob.UI;
    using Views.Purchases.Items;

    public class PurchaseIapItemWidget : StatefulWidget {
        public string PurchaseKey   { get; }
        public string StoreItemKey  { get; }
        public string CategoryKey   { get; }

        public bool HasNewMention { get; }

        public WidgetViewReference ViewReference = UiConstants.Views.Purchases.Items.Iap;

        public PurchaseIapItemWidget([NotNull] string purchaseKey, [NotNull] string categoryKey, [NotNull] string storeItemKey, bool hasNewMention) {
            this.PurchaseKey   = purchaseKey ?? throw new ArgumentNullException(nameof(purchaseKey));
            this.CategoryKey   = categoryKey ?? throw new ArgumentNullException(nameof(categoryKey));
            this.StoreItemKey  = storeItemKey ?? throw new ArgumentNullException(nameof(storeItemKey));
            this.HasNewMention = hasNewMention;
        }
    }

    public class PurchaseIapItemState : ViewState<PurchaseIapItemWidget>, IPurchasesIapItemState {
        [Inject] private readonly StoreItemsModel storeItemsModel;
        
        public override WidgetViewReference View => this.Widget.ViewReference;

        public string CategoryKey    => this.Widget.CategoryKey;
        public string PurchaseKey    => this.Widget.PurchaseKey;
        public string LocalizedPrice => this.purchasing.GetLocalizedPriceString(this.PurchaseKey);

        public bool HasNewMention => this.Widget.HasNewMention;

        public IState DropState => this.dropStateHolder.Value;

        private readonly GameDef gameDef;

        private readonly IPurchasing purchasing;
        private readonly World       world;
        private readonly StateHolder dropStateHolder;

        private PurchaseDef PurchaseDef => this.gameDef.Purchases.Get(this.PurchaseKey);

        private StoreItemModel StoreItemModel => this.storeItemsModel.Get(this.Widget.StoreItemKey);

        public PurchaseIapItemState(GameDef gameDef, IPurchasing purchasing, World world) {
            this.gameDef         = gameDef;
            this.purchasing      = purchasing;
            this.world           = world;
            this.dropStateHolder = this.CreateChild(this.BuildDropWidget);
        }

        public void Purchase() {
            StoreFeatureEvents.Purchase.Raise(new StoreFeatureEvents.PurchaseArgs {
                storeItemKey = this.Widget.StoreItemKey,
            });
        }

        private Widget BuildDropWidget(BuildContext context) {
            return new PurchasesDropWidget(this.StoreItemModel.ExtraDrops.ToList(), this.PurchaseKey, this.CategoryKey);
        }
    }
}