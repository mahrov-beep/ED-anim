namespace Game.UI.Views.GameInventory {
    using UniMob.UI;
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class GameInventoryEmptySlotItemView : AutoView<IGameInventoryEmptySlotItemState> {
        [SerializeField, Required] private UniMobDropZoneBehaviour assignDropZone;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("slot_type", () => this.State.SlotType, CharacterLoadoutSlots.PrimaryWeapon),
        };

        protected override void Awake() {
            base.Awake();

            this.assignDropZone.IsPayloadAcceptableDelegate = p => {
                if (!this.HasState) {
                    return false;
                }

                if (p is not DragAndDropPayloadItem itemEntity) {
                    return false;
                }
                
                this.assignDropZone.CanDrop = this.State.CanDrop(itemEntity);

                return this.State.CanAssignItem(itemEntity);
            };

            this.assignDropZone.OnAccept.AddListener(payload => {
                if (this.HasState && payload is DragAndDropPayloadItem itemEntity) {
                    this.State.OnAssignItem(itemEntity);
                }
            });
        }
    }

    public interface IGameInventoryEmptySlotItemState : IViewState {
        string SlotType { get; }

        bool CanDrop(DragAndDropPayloadItem payload);

        bool CanAssignItem(DragAndDropPayloadItem payload);

        void OnAssignItem(DragAndDropPayloadItem payload);
    }
}