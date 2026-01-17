namespace Game.UI.Widgets.Storage {
    using System;
    using Header;
    using Purchases;
    using Shared;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Store;

    [RequireFieldsInit]
    public class StoreWidget : StatefulWidget {
        public Action OnClose { get; set; }
    }

    public class StoreState : ViewState<StoreWidget>, IStoreState {
        public override WidgetViewReference View => UiConstants.Views.Purchases.Store;

        [Atom] public IState Header => this.RenderChild(_ => new Row {
            CrossAxisSize      = AxisSize.Max,
            MainAxisSize       = AxisSize.Max,
            CrossAxisAlignment = CrossAxisAlignment.Center,
            MainAxisAlignment  = MainAxisAlignment.End,
            Size               = WidgetSize.Stretched,
            Children = {
                new HeaderCurrencyWidget(SharedConstants.Game.Currencies.LOADOUT_TICKETS),
                new HeaderCurrencyWidget(SharedConstants.Game.Currencies.BADGES),
                new HeaderCurrencyWidget(SharedConstants.Game.Currencies.BUCKS),
                new HeaderCurrencyWidget(SharedConstants.Game.Currencies.CRYPT),
            },
        });
        
        [Atom] public IState PurchasesWidget => this.RenderChild(_ => new PurchasesStoreWidget());

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }
    }
}