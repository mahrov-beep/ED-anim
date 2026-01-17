namespace Game.UI.Widgets.Storage.TraderShop {
    using Domain.items;
    using Items;
    using Multicast;
    using Quantum;
    using Shared;
    using Shared.UserProfile.Data;
    using UniMob.UI;
    using Views;

    [RequireFieldsInit]
    public class RewardStorageItemWidget : StatefulWidget, IStorageItemWidget {
        public GameSnapshotLoadoutItem Item;
    }

    public class RewardStorageItemState : StorageItemState<RewardStorageItemWidget> {
        [Inject] private GameDef       gameDef;
        [Inject] private ItemsModel    itemsModel;
        [Inject] private SdUserProfile userProfile;

        public override bool Draggable => false;

        protected override ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Widget.Item.ItemKey)
        );

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() => null;

        public override void Select() {
        }
        
        public override void OnDoubleClick() {
        }

        protected override Widget BuildDetails(BuildContext context) {
            return new SnapshotItemDetailsWidget {
                Item = this.Widget.Item,
            };
        }
    }
}