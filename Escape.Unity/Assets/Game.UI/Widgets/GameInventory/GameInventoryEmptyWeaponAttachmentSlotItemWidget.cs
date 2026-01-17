namespace Game.UI.Widgets.GameInventory {
    using Multicast;
    using Quantum;
    using Services.Photon;
    using UniMob.UI;
    using Views;
    using Views.GameInventory;

    [RequireFieldsInit]
    public class GameInventoryEmptyWeaponAttachmentSlotItemWidget : StatefulWidget {
        public CharacterLoadoutSlots SlotType;
        public WeaponAttachmentSlots WeaponSlotType;
    }

    public class GameInventoryEmptyWeaponAttachmentSlotItemState : ViewState<GameInventoryEmptyWeaponAttachmentSlotItemWidget>, IGameInventoryEmptyWeaponAttachmentSlotItemState {
        [Inject] private GameInventoryApi gameInventoryApi;

        public override WidgetViewReference View => UiConstants.Views.GameInventory.WeaponAttachmentSlotEmpty;

        public string WeaponSlotType => EnumNames<WeaponAttachmentSlots>.GetName(this.Widget.WeaponSlotType);

        public bool CanAssignItem(DragAndDropPayloadItem payload) {
            return this.gameInventoryApi.CanAssignItemToWeaponSlot(payload, this.Widget.SlotType, this.Widget.WeaponSlotType);
        }

        public void OnAssignItem(DragAndDropPayloadItem payload) {
            this.gameInventoryApi.AssignItemToWeaponSlot(payload, this.Widget.SlotType, this.Widget.WeaponSlotType);
        }
    }
}