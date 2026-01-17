namespace Game.UI.Views.GameInventory {
    using UniMob.UI;
    using Multicast;
    using Quantum;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameInventoryTrashItemView : AutoView<IGameInventoryTrashItemState> {
        [SerializeField, Required] private ViewPanel detailsViewPanel;

        [SerializeField, Required] private UniMobSwipeBehaviour swipeBehaviour;

        [SerializeField, Required] private UniMobDropZoneBehaviour trashDropZone;

        [SerializeField, Required] private HoldToActionButton useButton;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("item_key", () => this.State.ItemKey, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("item_icon", () => this.State.ItemIcon, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("is_selected", () => this.State.IsSelected, true),
            this.Variable("can_be_used", () => this.State.CanBeUsed, true),
            this.Variable("weight", () => this.State.Weight, 1.5f),
            this.Variable("item_rarity", () => this.State.ItemRarity, ERarityType.Common),
            this.Variable("usages_remaining", () => this.State.UsagesRemaining, 0),
            this.Variable("usages_max", () => this.State.UsagesMax, 0),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("select", () => this.State.Select()),
            this.Event("use", () => this.State.Use()),
        };

        protected override void Awake() {
            base.Awake();

       //     this.trashDropZone.IsPayloadAcceptableDelegate = p => {
       //         if (!this.HasState) {
       //             return false;
       //         }
//
       //         if (p is not DragAndDropPayloadItem itemEntity) {
       //             return false;
       //         }
       //         
       //         var canMoveItemToTrash = this.State.CanMoveItemToTrash(itemEntity);
//
       //         this.trashDropZone.CanDrop = this.State.IsEnoughSpace(itemEntity);
//
       //         return canMoveItemToTrash;
       //     };
       //     this.trashDropZone.OnAccept.AddListener(payload => {
       //         if (this.HasState && payload is DragAndDropPayloadItem itemEntity) {
       //             if (!this.State.IsEnoughSpace(itemEntity)) {
       //                 return;
       //             }
       //             
       //             this.State.OnMoveItemToTrash(itemEntity);
       //         }
       //     });

            this.useButton.holdEnded.AddListener(this.HandleHoldEnded);
            this.useButton.fastClick.AddListener(this.HandleFastClick);
            this.useButton.doubleClick.AddListener(this.HandleDoubleClick);
            this.useButton.holdStarted.AddListener(this.HandleHoldStarted);
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

            bool isHudButton = this.State.IsHudButton;

            this.useButton.canHold      = this.State.CanBeUsed && !isHudButton;
            this.useButton.holdDuration = this.State.HoldToUseSeconds;

            this.detailsViewPanel.Render(this.State.Details);

            if (isHudButton) {
                return;
            }
            
            this.rectTransform.rotation      = this.State.Rotated ? Quaternion.Euler(Vector3.forward * -90) : Quaternion.identity;
            this.rectTransform.pivot         = this.State.Rotated ? Vector2.zero : Vector2.up;
            this.rectTransform.localPosition = Vector3.zero;
        }

        private void HandleHoldEnded() {
            if (!this.HasState) {
                return;
            }

            this.State.Use();
        }

        private void HandleFastClick() {
            if (!this.HasState) {
                return;
            }

            if (this.State.IsHudButton) {
                if (this.State.CanBeUsed) {
                    this.State.Use();
                }

                return;
            }

            this.State.Select();
        }

        private void HandleHoldStarted() {
            if (!this.HasState) {
                return;
            }

            if (this.State.IsHudButton || !this.State.CanBeUsed) {
                return;
            }

            this.State.StartHold();

            if (!this.State.UseOnHoldStart) {
                return;
            }

            this.useButton.CancelHold();
            this.State.Use();
        }

        private void HandleDoubleClick() {
            if (!this.HasState) {
                return;
            }

            if (this.State.IsHudButton) {
                return;
            }

            this.State.DoubleClick();
        }
    }

    public interface IGameInventoryTrashItemState : IViewState {
        string ItemKey    { get; }
        string ItemIcon   { get; }
        bool   IsSelected { get; }
        float  Weight     { get; }
        string ItemRarity { get; }

        bool  CanBeUsed        { get; }
        bool  Rotated          { get; }
        bool  IsHudButton      { get; }
        bool  UseOnHoldStart   { get; }
        float HoldToUseSeconds { get; }

        int UsagesRemaining { get; }
        int UsagesMax       { get; }

        IState Details { get; }

        DragAndDropPayloadItem GetDragAndDropItemPayload();

        void Select();
        void StartHold();
        void Use();
        void DoubleClick();
    }
}