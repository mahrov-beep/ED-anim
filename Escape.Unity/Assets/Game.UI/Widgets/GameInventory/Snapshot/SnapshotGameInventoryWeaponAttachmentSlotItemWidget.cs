namespace Game.UI.Widgets.GameInventory.Snapshot {
    using Controllers.Features.SelectedItemInfo;
    using Multicast;
    using Quantum;
    using Views;

    [RequireFieldsInit]
    public class SnapshotGameInventoryWeaponAttachmentSlotItemWidget : GameInventoryWeaponAttachmentSlotItemWidget {
        public CharacterLoadoutSlots               SlotType;
        public WeaponAttachmentSlots               WeaponSlotType;
        public GameSnapshotLoadoutWeaponAttachment AttachmentSnapshot;
    }

    public class SnapshotGameInventoryWeaponAttachmentSlotItemState :
        GameInventoryWeaponAttachmentSlotItemState<SnapshotGameInventoryWeaponAttachmentSlotItemWidget> {
        protected override WeaponAttachmentSlots AssignedSlot => this.Widget.WeaponSlotType;

        protected override ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Widget.AttachmentSnapshot.ItemKey)
        );

        public override int UsagesRemaining => Item.GetRemainingUsages(this.Widget.AttachmentSnapshot.Used, this.ItemAsset.MaxUsages);
        public override int UsagesMax       => this.ItemAsset.MaxUsages;

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() {
            return null;
        }

        public override void Select() {
            SelectedItemInfoFeatureEvents.Select.Raise(new SelectedItemInfoFeatureEvents.SelectArgs {
                ItemKey             = this.ItemAsset.ItemKey,
                ItemEntity          = EntityRef.None,
                ItemSnapshot        = this.Widget.AttachmentSnapshot.ToItem(),
                Position            = WidgetPosition.Position.Left,
                IsTakeButtonVisible = false,
            });
        }

        public override bool CanAssignItem(DragAndDropPayloadItem payload) {
            return false;
        }

        public override void OnAssignItem(DragAndDropPayloadItem payload) {
        }
    }
}