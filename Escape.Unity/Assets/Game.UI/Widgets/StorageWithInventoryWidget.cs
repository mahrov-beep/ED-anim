namespace Game.UI.Widgets {
    using System;
    using GameInventory;
    using Storage.NearbyItems;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class StorageWithInventoryWidget : StatefulWidget {
        public Action OnClose;
        public Action OnIncrementLoadout;
        public Action OnDecrementLoadout;
        
        public bool HasSomeLoadouts;
        
        public int LoadoutIndex;
        public int LoadoutCount;
    }

    public class StorageWithInventoryState : HocState<StorageWithInventoryWidget> {
        public override Widget Build(BuildContext context) {
            return new ZStack {
                Children = {
                    new NearbyItemsStorageWidget(),
                    new GameInventoryWidget {
                        OnClose               = this.Widget.OnClose,
                        OnIncrementLoadout    = this.Widget.OnIncrementLoadout,
                        OnDecrementLoadout    = this.Widget.OnDecrementLoadout,
                        ShowItemsThrowZone    = false,
                        NoDraggingInInventory = false,
                        IgnoreNearby          = true,
                        HasSomeLoadouts       = this.Widget.HasSomeLoadouts,
                        LoadoutIndex          = this.Widget.LoadoutIndex,
                        LoadoutCount          = this.Widget.LoadoutCount,
                    },
                },
            };
        }
    }
}