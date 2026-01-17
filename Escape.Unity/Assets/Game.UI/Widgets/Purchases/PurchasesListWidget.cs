namespace Game.UI.Widgets.Purchases {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Models.Purchases;
    using Multicast;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    public class PurchasesListWidget : StatefulWidget {
        public List<string> PurchasesItems { get; }

        public string ContextCategoryKey { get; }

        public PurchasesListWidget(List<string> purchasesItems, string contextCategoryKey) {
            this.PurchasesItems     = purchasesItems;
            this.ContextCategoryKey = contextCategoryKey;
        }
    }

    public class PurchasesListState : HocState<PurchasesListWidget> {
        [Inject] private readonly StoreItemsModel storeItemsModel;

        public override Widget Build(BuildContext context) {
            return this.BuildPurchases(context);
        }

        private Widget BuildPurchases(BuildContext context) {
            return new Row() {
                CrossAxisAlignment = CrossAxisAlignment.Center,
                MainAxisAlignment  = MainAxisAlignment.Center,
                Children = new List<Widget>() {
                    this.Widget.PurchasesItems
                        .Select(it => this.storeItemsModel.Get(it))
                        .Select(it => PurchasesStoreWidget.BuildPurchaseItem(it, this.Widget.ContextCategoryKey)),
                },
            };
        }
    }
}