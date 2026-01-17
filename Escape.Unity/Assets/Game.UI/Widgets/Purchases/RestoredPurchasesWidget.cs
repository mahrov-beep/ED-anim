namespace Game.UI.Widgets.Subscription {
    using System;
    using UniMob.UI;
    using Views.Subscription;

    public class RestoredPurchasesWidget : StatefulWidget {
        public Action OnClose { get; set; }
    }

    public class RestoredPurchasesState : ViewState<RestoredPurchasesWidget>, IRestoredPurchasesState {
        public void Close() {
            this.Widget.OnClose?.Invoke();
        }
        
        public override WidgetViewReference View => UiConstants.Views.Subscription.RestoredPurchasesView;
    }
}