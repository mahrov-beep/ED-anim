namespace Game.UI.Widgets {
    using System;
    using Storage.TraderShop;
    using TraderShop;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class StorageWithTraderShopWidget : StatefulWidget {
        public Action OnClose;
    }

    public class StorageWithTraderShopState : HocState<StorageWithTraderShopWidget> {
        public override Widget Build(BuildContext context) {
            return new ZStack {
                Children = {
                    new TraderShopStorageWidget(),
                    new TraderShopWidget {
                        OnClose = this.Widget.OnClose,
                    },
                },
            };
        }
    }
}