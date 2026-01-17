namespace Game.UI.Widgets.GameInventory.Snapshot {
    using System;
    using System.Linq;
    using Controllers.Features.SelectedItemInfo;
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views;

    [RequireFieldsInit]
    public class SnapshotGameInventorySlotItemWidget : GameInventorySlotItemWidget {
        public CharacterLoadoutSlots   SlotType;
        public GameSnapshotLoadoutItem ItemSnapshot;
    }

    public class SnapshotGameInventorySlotItemState : GameInventorySlotItemState<SnapshotGameInventorySlotItemWidget> {
        protected override CharacterLoadoutSlots AssignedSlot => this.Widget.SlotType;

        protected override ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Widget.ItemSnapshot.ItemKey)
        );

        public override int UsagesRemaining => Item.GetRemainingUsages(this.Widget.ItemSnapshot.Used, this.ItemAsset.MaxUsages);
        public override int UsagesMax       => this.ItemAsset.MaxUsages;

        public override bool IsWeaponAttachment(DragAndDropPayloadItem payload) {
            return false;
        }

        public override bool CanAssignItem(DragAndDropPayloadItem payload) {
            return false;
        }
        
        public override bool CanDropItem(DragAndDropPayloadItem payload) {
            return false;
        }

        public override void OnAssignItem(DragAndDropPayloadItem payload) {
        }

        public override void Select() {
            SelectedItemInfoFeatureEvents.Select.Raise(new SelectedItemInfoFeatureEvents.SelectArgs {
                ItemKey             = this.ItemAsset.ItemKey,
                ItemEntity          = EntityRef.None,
                ItemSnapshot        = this.Widget.ItemSnapshot,
                Position            = WidgetPosition.Position.Left,
                IsTakeButtonVisible = false,
            });
        }

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() {
            return null;
        }

        protected override Widget BuildAttachments(BuildContext context) {
            if (this.ItemAsset is WeaponItemAsset weaponItemAsset) {
                var schema      = weaponItemAsset.attachmentsSchema;
                var attachments = this.Widget.ItemSnapshot.WeaponAttachments ?? Array.Empty<GameSnapshotLoadoutWeaponAttachment>();

                if (schema != null) {
                    return this.BuildAttachmentsLayout(schema.slots.Select(slot => slot.ToInt() < attachments.Length && attachments[slot.ToInt()] is {} attachment
                        ? this.BuildWeaponAttachmentSlot(slot, attachment)
                        : this.BuildEmptyWeaponAttachmentSlot(slot)), schema.itemsCountOnFirstLineInInventory);
                }
            }

            return new Empty();
        }

        private Widget BuildWeaponAttachmentSlot(WeaponAttachmentSlots weaponAttachmentSlot, GameSnapshotLoadoutWeaponAttachment attachment) {
            return new SnapshotGameInventoryWeaponAttachmentSlotItemWidget {
                SlotType           = this.AssignedSlot,
                WeaponSlotType     = weaponAttachmentSlot,
                AttachmentSnapshot = attachment,

                Key = Key.Of(weaponAttachmentSlot),
            };
        }

        private Widget BuildEmptyWeaponAttachmentSlot(WeaponAttachmentSlots weaponAttachmentSlot) {
            return new GameInventoryEmptyWeaponAttachmentSlotItemWidget {
                SlotType       = this.AssignedSlot,
                WeaponSlotType = weaponAttachmentSlot,

                Key = Key.Of(weaponAttachmentSlot),
            };
        }
    }
}