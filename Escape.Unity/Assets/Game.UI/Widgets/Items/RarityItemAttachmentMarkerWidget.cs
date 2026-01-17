namespace Game.UI.Widgets.Items {
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using Views.items;

    [RequireFieldsInit]
    public class RarityItemAttachmentMarkerWidget : StatefulWidget {
        public ERarityType Rarity;
    }

    public class RarityItemAttachmentMarkerState : ViewState<RarityItemAttachmentMarkerWidget>, IItemAttachmentMarkerState {
        public override WidgetViewReference View => UiConstants.Views.Items.AttachmentMarker;

        public string ItemRarity => EnumNames<ERarityType>.GetName(this.Widget.Rarity);
    }
}