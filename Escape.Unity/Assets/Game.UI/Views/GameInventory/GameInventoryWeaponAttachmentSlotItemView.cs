namespace Game.UI.Views.GameInventory {
    using UniMob.UI;
    using Multicast;
    using Quantum;
    using Shared;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class GameInventoryWeaponAttachmentSlotItemView : AutoView<IGameInventoryWeaponAttachmentSlotItemState> {
        [SerializeField, Required] private UniMobSwipeBehaviour    swipeBehaviour;
        [SerializeField, Required] private UniMobDropZoneBehaviour assignDropZone;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("weapon_slot_type", () => this.State.WeaponSlotType, WeaponAttachmentSlots.Magazine),
            this.Variable("item_key", () => this.State.ItemKey, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("item_icon", () => this.State.ItemIcon, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("is_selected", () => this.State.IsSelected, true),
            this.Variable("item_rarity", () => this.State.ItemRarity, ERarityType.Common),
            this.Variable("usages_remaining", () => this.State.UsagesRemaining, 0),
            this.Variable("usages_max", () => this.State.UsagesMax, 0),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("select", () => this.State.Select()),
        };

        protected override void Awake() {
            base.Awake();

            this.assignDropZone.IsPayloadAcceptableDelegate = p => {
                if (!this.HasState) {
                    return false;
                }

                return p is DragAndDropPayloadItem itemEntity && this.State.CanAssignItem(itemEntity);
            };

            this.assignDropZone.OnAccept.AddListener(payload => {
                if (this.HasState && payload is DragAndDropPayloadItem itemEntity) {
                    this.State.OnAssignItem(itemEntity);
                }
            });
        }

        protected override void Activate() {
            base.Activate();

            this.swipeBehaviour.CreateDragAndDropPayloadDelegate = this.State.GetDragAndDropItemPayload;
        }

        protected override void Deactivate() {
            base.Deactivate();

            this.swipeBehaviour.CreateDragAndDropPayloadDelegate = null;
        }
    }

    public interface IGameInventoryWeaponAttachmentSlotItemState : IViewState {
        string WeaponSlotType { get; }
        string ItemKey        { get; }
        string ItemIcon       { get; }
        bool   IsSelected     { get; }
        string ItemRarity     { get; }

        int UsagesRemaining { get; }
        int UsagesMax       { get; }

        DragAndDropPayloadItem GetDragAndDropItemPayload();

        bool CanAssignItem(DragAndDropPayloadItem payload);

        void OnAssignItem(DragAndDropPayloadItem payload);

        void Select();
    }
}