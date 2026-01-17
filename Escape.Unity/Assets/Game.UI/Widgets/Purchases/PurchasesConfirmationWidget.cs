namespace Game.UI.Widgets.Purchases {
    using System;
    using Common;
    using Domain.Models.Purchases;
    using Multicast;
    using UniMob.UI;

    public class PurchasesConfirmationWidget : StatefulWidget {
        public string StoreItemKey { get; }

        public Action<bool> OnResult { get; set; }

        public PurchasesConfirmationWidget(string storeItemKey) {
            this.StoreItemKey = storeItemKey;
        }
    }

    public class PurchasesConfirmationState : HocState<PurchasesConfirmationWidget> {
        [Inject] private StoreItemsModel storeItemsModel;

        public override Widget Build(BuildContext context) {
            var item = this.storeItemsModel.Get(this.Widget.StoreItemKey);

            return new PopupWidget("PURCHASE_CONFIRMATION_TITLE") {
                OnClose = () => this.Widget.OnResult?.Invoke(false),
                Content = new ConfirmationScreenWidget() {
                    OnResult = this.Widget.OnResult,
                    Content  = PurchasesStoreWidget.BuildPurchaseItem(item, item.Category),
                },
            };
        }
    }
}