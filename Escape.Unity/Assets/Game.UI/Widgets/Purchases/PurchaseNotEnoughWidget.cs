namespace Game.UI.Widgets.Purchases {
    using System;
    using UniMob.UI;
    using Views.Purchases;

    public class PurchaseNotEnoughWidget : StatefulWidget {
        public Action OnClose    { get; set; }
    }

    public class PurchaseNotEnoughState : ViewState<PurchaseNotEnoughWidget>, IPurchaseNotEnoughState {
        public override WidgetViewReference View => UiConstants.Views.Purchases.PurchaseNotEnoughView;
        public void Close() {
            this.Widget.OnClose?.Invoke();
        }
    }
}