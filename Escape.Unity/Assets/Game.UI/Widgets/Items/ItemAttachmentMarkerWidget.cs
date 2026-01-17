namespace Game.UI.Widgets.Items {
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using Views.items;

    [RequireFieldsInit]
    public class ItemAttachmentMarkerWidget : StatefulWidget {
        public string ItemKey;
    }

    public class ItemAttachmentMarkerState : ViewState<ItemAttachmentMarkerWidget>, IItemAttachmentMarkerState {
        private ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Widget.ItemKey));

        public override WidgetViewReference View => UiConstants.Views.Items.AttachmentMarker;

        public string ItemRarity => EnumNames<ERarityType>.GetName(this.ItemAsset.rarity);
    }
}