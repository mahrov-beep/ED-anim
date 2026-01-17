namespace Game.UI.Views.GameInventory {
    using UniMob.UI;
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class GameInventoryEmptyWeaponAttachmentSlotItemView : AutoView<IGameInventoryEmptyWeaponAttachmentSlotItemState> {
        [SerializeField, Required] private UniMobDropZoneBehaviour assignDropZone;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("weapon_slot_type", () => this.State.WeaponSlotType, WeaponAttachmentSlots.Magazine),
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
    }

    public interface IGameInventoryEmptyWeaponAttachmentSlotItemState : IViewState {
        string WeaponSlotType { get; }

        bool CanAssignItem(DragAndDropPayloadItem payload);

        void OnAssignItem(DragAndDropPayloadItem payload);
    }
}