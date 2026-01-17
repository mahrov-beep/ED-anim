namespace Game.UI.Widgets.Storage {
    using Quantum;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views;

    [RequireFieldsInit]
    public class StorageItemSimpleWidget : StatefulWidget, IStorageItemWidget {
        public string ItemKey;
        public Widget Details;
    }

    public class StorageItemSimpleState : StorageItemState<StorageItemSimpleWidget> {
        protected override ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Widget.ItemKey)
        );

        public override bool Draggable => false;

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() {
            return null;
        }

        public override void Select() {
        }
        
        public override void OnDoubleClick() {
        }

        protected override Widget BuildDetails(BuildContext context) {
            return this.Widget.Details;
        }
    }
}