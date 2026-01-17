namespace Game.UI.Widgets.GameInventory.Simulation {
    using System.Linq;
    using Controllers.Features.SelectedItemInfo;
    using Domain;
    using Domain.GameInventory;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using SoundEffects;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views;

    [RequireFieldsInit]
    public class SimulationGameInventorySlotItemWidget : GameInventorySlotItemWidget {
        public GameInventorySlotItemModel Model;

        public bool NoDragging;
    }

    public class SimulationGameInventorySlotItemState : GameInventorySlotItemState<SimulationGameInventorySlotItemWidget> {
        [Inject] private PhotonService       photonService;
        [Inject] private GameInventoryModel  gameInventoryModel;
        [Inject] private ISoundEffectService soundEffectService;

        protected override CharacterLoadoutSlots AssignedSlot => this.Widget.Model.SlotType;

        private EntityRef ItemEntity => this.Widget.Model.ItemEntity;
        private Item      Item       => this.photonService.PredictedFrame!.Get<Item>(this.ItemEntity);

        protected override ItemAsset ItemAsset => this.photonService.PredictedFrame!.FindAsset(this.Item.Asset);

        public override bool IsSelected => this.gameInventoryModel.SelectedItem == this.ItemEntity;
        public override bool IsBlocked  => this.Widget.Model.IsBlocked.Value;

        public override int UsagesRemaining => this.Widget.Model.RemainingUsages.Value;
        public override int UsagesMax       => this.ItemAsset.MaxUsages;

        public override void Select() {
            SelectedItemInfoFeatureEvents.Select.Raise(new SelectedItemInfoFeatureEvents.SelectArgs {
                ItemKey             = this.ItemAsset.ItemKey,
                ItemEntity          = this.ItemEntity,
                ItemSnapshot        = null,
                Position            = WidgetPosition.Position.Left,
                IsTakeButtonVisible = false,
            });
        }

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() {
            return this.Widget.NoDragging ? null : new DragAndDropPayloadItemEntityFromSlot {
                ItemEntity = this.ItemEntity,
                SourceSlot = this.Widget.Model.SlotType,
            };
        }

        public override bool IsWeaponAttachment(DragAndDropPayloadItem payload) => payload switch {
            DragAndDropPayloadItemEntityFromTetris fromTetris => this.IsWeaponAttachmentRef(fromTetris.ItemEntity),
            DragAndDropPayloadItemEntityFromSlot fromSlot => this.IsWeaponAttachmentRef(fromSlot.ItemEntity),
            DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot => this.IsWeaponAttachmentRef(fromWeaponSlot.ItemEntity),
            _ => false,
        };

        private bool IsWeaponAttachmentRef(EntityRef itemEntity) {
            return this.photonService.PredictedFrame is { } f &&
                   f.TryGet(itemEntity, out Item item) &&
                   f.FindAsset(item.Asset) is WeaponAttachmentItemAsset;
        }


        public override bool CanAssignItem(DragAndDropPayloadItem payload) {
            if (payload is DragAndDropPayloadItemEntityFromSlot fromSlot && fromSlot.ItemEntity == this.ItemEntity) {
                return true;
            }

            return this.gameInventoryApi.CanAssignItemToSlot(payload, this.Widget.Model.SlotType);
        }

        public override bool CanDropItem(DragAndDropPayloadItem payload) {
            if (this.IsWeaponAttachment(payload)) {
                return true;
            }

            if (payload is DragAndDropPayloadItemEntityFromTetris fromTetris) {
                return this.gameInventoryApi.CanSwapItem(this.AssignedSlot, fromTetris.ItemEntity);
            }
            
            return true;
        }

        public override void OnAssignItem(DragAndDropPayloadItem payload) {
            if (payload is DragAndDropPayloadItemEntityFromSlot fromSlot && fromSlot.ItemEntity == this.ItemEntity) {
                return;
            }

            this.gameInventoryApi.AssignItemToSlot(payload, this.Widget.Model.SlotType);

            this.soundEffectService.PlayOneShot(CoreConstants.SoundEffectKeys.Equip);
        }

        protected override Widget BuildAttachments(BuildContext context) {
            if (this.ItemAsset is WeaponItemAsset weaponItemAsset) {
                var weaponAttachments = this.gameInventoryModel.GetWeaponAttachmentsModel(this.Widget.Model.SlotType);
                if (weaponAttachments != null) {
                    var attachmentSchema = weaponItemAsset.attachmentsSchema;

                    return this.BuildAttachmentsLayout(attachmentSchema.slots.Select(slot => weaponAttachments.TryGetSlotItem(slot, out var model)
                        ? this.BuildWeaponAttachmentSlot(model)
                        : this.BuildEmptyWeaponAttachmentSlot(slot)), attachmentSchema.itemsCountOnFirstLineInInventory);
                }
            }

            return new Empty();
        }

        private Widget BuildWeaponAttachmentSlot(GameInventoryWeaponAttachmentSlotItemModel model) {
            return new SimulationGameInventoryWeaponAttachmentSlotItemWidget {
                Model = model,
                Key   = Key.Of(model),
                NoDragging = this.Widget.NoDragging,
            };
        }

        private Widget BuildEmptyWeaponAttachmentSlot(WeaponAttachmentSlots weaponAttachmentSlot) {
            return new GameInventoryEmptyWeaponAttachmentSlotItemWidget {
                SlotType       = this.Widget.Model.SlotType,
                WeaponSlotType = weaponAttachmentSlot,
                Key            = Key.Of(weaponAttachmentSlot),
            };
        }
    }
}