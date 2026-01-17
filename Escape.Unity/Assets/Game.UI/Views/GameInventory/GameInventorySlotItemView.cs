namespace Game.UI.Views.GameInventory {
    using System.Linq;
    using UniMob.UI;
    using Multicast;
    using Quantum;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameInventorySlotItemView : AutoView<IGameInventorySlotItemState> {
        [SerializeField, Required] private UniMobSwipeBehaviour    swipeBehaviour;
        [SerializeField, Required] private UniMobDropZoneBehaviour assignDropZone;

        [SerializeField, Required] private ViewPanel attachmentsPanel;

        [SerializeField] private GameObject highlightAttachmentNotValidForCurrentWeapon;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("slot_type", () => this.State.SlotType, CharacterLoadoutSlots.PrimaryWeapon),
            this.Variable("item_key", () => this.State.ItemKey, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("item_icon", () => this.State.ItemIcon, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("is_selected", () => this.State.IsSelected, true),
            this.Variable("item_rarity", () => this.State.ItemRarity, ERarityType.Common),
            this.Variable("is_blocked", () => this.State.IsBlocked),
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

                if (p is not DragAndDropPayloadItem itemEntity) {
                    return false;
                }

                this.assignDropZone.CanDrop = this.State.CanDropItem(itemEntity);

                return this.State.CanAssignItem(itemEntity);
            };

            this.assignDropZone.OnAccept.AddListener(payload => {
                if (this.HasState && payload is DragAndDropPayloadItem itemEntity) {
                    this.State.OnAssignItem(itemEntity);
                }
            });

            this.assignDropZone.CustomHighlightDelegate = (payloads, isAccepted) => {
                if (this.highlightAttachmentNotValidForCurrentWeapon) {
                    var attachmentNotValidForCurrentWeapon =
                        !isAccepted &&
                        this.HasState &&
                        payloads.Count > 0 &&
                        payloads.All(it => it is DragAndDropPayloadItem payloadItem && this.State.IsWeaponAttachment(payloadItem));
                    this.highlightAttachmentNotValidForCurrentWeapon.SetActive(attachmentNotValidForCurrentWeapon);
                }
            };
        }

        protected override void Activate() {
            base.Activate();

            this.swipeBehaviour.CreateDragAndDropPayloadDelegate = this.State.GetDragAndDropItemPayload;
        }

        protected override void Deactivate() {
            base.Deactivate();

            this.swipeBehaviour.CreateDragAndDropPayloadDelegate = null;
        }

        protected override void Render() {
            base.Render();

            this.swipeBehaviour.enabled = !this.State.IsBlocked;

            this.attachmentsPanel.Render(this.State.Attachments);
        }
    }

    public interface IGameInventorySlotItemState : IViewState {
        string SlotType   { get; }
        string ItemKey    { get; }
        string ItemIcon   { get; }
        bool   IsSelected { get; }
        string ItemRarity { get; }

        bool IsBlocked { get; }
        
        int UsagesRemaining { get; }
        int UsagesMax       { get; }

        IState Attachments { get; }

        DragAndDropPayloadItem GetDragAndDropItemPayload();

        bool CanAssignItem(DragAndDropPayloadItem payload);
        bool CanDropItem(DragAndDropPayloadItem payload);

        void OnAssignItem(DragAndDropPayloadItem payload);

        bool IsWeaponAttachment(DragAndDropPayloadItem payload);

        void Select();
    }
}