namespace Game.UI.Widgets.Purchases {
    using System;
    using System.Collections.Generic;
    using Domain;
    using UniMob.UI;
    using Views.Purchases;

    public class PurchasesTopUpWidget : StatefulWidget {
        public Action       OnClose        { get; set; }
        public List<string> PurchasesItems { get; set; }
    }

    public class PurchasesTopUpState : ViewState<PurchasesTopUpWidget>, IPurchasesTopUpState {
        private const string CONTEXT_CATEGORY_KEY = "purchase_top_up";

        public override WidgetViewReference View => UiConstants.Views.Purchases.TopUpView;

        public IState ContentState => this.stateHolder.Value;

        public readonly StateHolder stateHolder;


        public PurchasesTopUpState() {
            this.stateHolder = this.CreateChild(this.BuildPurchases);
        }

        private Widget BuildPurchases(BuildContext context) {
            return new PurchasesListWidget(this.Widget.PurchasesItems ?? new List<string>(), CONTEXT_CATEGORY_KEY);
        }

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }
    }
}