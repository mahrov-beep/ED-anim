namespace Game.UI.Widgets.GameInventory.Simulation {
    using Controllers.Features.SelectedItemInfo;
    using Domain.GameInventory;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using Views;

    [RequireFieldsInit]
    public class SimulationGameInventoryWeaponAttachmentSlotItemWidget : GameInventoryWeaponAttachmentSlotItemWidget {
        public GameInventoryWeaponAttachmentSlotItemModel Model;

        public bool NoDragging;
    }

    public class SimulationGameInventoryWeaponAttachmentSlotItemState :
        GameInventoryWeaponAttachmentSlotItemState<SimulationGameInventoryWeaponAttachmentSlotItemWidget> {
        [Inject] private PhotonService      photonService;
        [Inject] private GameInventoryModel gameInventoryModel;

        private EntityRef ItemEntity => this.Widget.Model.ItemEntity;
        private Item      Item       => this.photonService.PredictedFrame!.Get<Item>(this.ItemEntity);

        protected override ItemAsset ItemAsset => this.photonService.PredictedFrame!.FindAsset(this.Item.Asset);

        protected override WeaponAttachmentSlots AssignedSlot => this.Widget.Model.WeaponSlotType;

        public override bool IsSelected => this.gameInventoryModel.SelectedItem == this.ItemEntity;

        public override int UsagesRemaining =>  this.Widget.Model.RemainingUsages.Value;
        public override int UsagesMax       => this.ItemAsset.MaxUsages;

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() {
            return this.Widget.NoDragging ? null : new DragAndDropPayloadItemEntityFromWeaponSlot {
                ItemEntity                 = this.ItemEntity,
                SourceSlot                 = this.Widget.Model.SlotType,
                SourceWeaponAttachmentSlot = this.Widget.Model.WeaponSlotType,
            };
        }

        public override void Select() {
            SelectedItemInfoFeatureEvents.Select.Raise(new SelectedItemInfoFeatureEvents.SelectArgs {
                ItemKey             = this.ItemAsset.ItemKey,
                ItemEntity          = this.ItemEntity,
                ItemSnapshot        = null,
                Position            = WidgetPosition.Position.Left,
                IsTakeButtonVisible = false,
            });
        }

        public override bool CanAssignItem(DragAndDropPayloadItem payload) {
            if (payload is DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot && fromWeaponSlot.ItemEntity == this.ItemEntity) {
                return true;
            }

            return this.gameInventoryApi.CanAssignItemToWeaponSlot(payload, this.Widget.Model.SlotType, this.Widget.Model.WeaponSlotType);
        }

        public override void OnAssignItem(DragAndDropPayloadItem payload) {
            if (payload is DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot && fromWeaponSlot.ItemEntity == this.ItemEntity) {
                return;
            }

            this.gameInventoryApi.AssignItemToWeaponSlot(payload, this.Widget.Model.SlotType, this.Widget.Model.WeaponSlotType);
        }
    }
}