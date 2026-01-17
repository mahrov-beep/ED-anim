namespace Game.UI.Widgets.Purchases {
    using System.Collections.Generic;
    using UniMob.UI;
    using Views.Purchases;

    public class PurchasesStoreCategoryWidget : StatefulWidget {
        public string StoreCategoryKey { get; set; }

        public PurchasesStoreCategoryWidget(Key key) {
            this.Key = key;
        }
    }

    public class PurchasesStoreCategoryWidgetState : ViewState<PurchasesStoreCategoryWidget>, IPurchasesStoreCategoryState {
        public static readonly Dictionary<string, Key> KeyLookup = new();

        public override WidgetViewReference View => UiConstants.Views.Purchases.StoreCategory;

        public string StoreCategoryKey => this.Widget.StoreCategoryKey;

        public override void InitState() {
            base.InitState();

            KeyLookup[this.Widget.StoreCategoryKey] = this.Widget.Key;
        }

        public override void Dispose() {
            base.Dispose();

            KeyLookup.Remove(this.Widget.StoreCategoryKey);
        }
    }
}