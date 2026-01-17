namespace Game.UI.Views.Storage {
    using UniMob.UI;
    using Multicast;
    using Multicast.Numerics;
    using Quantum;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class StorageItemView : AutoView<IStorageItemState> {
        [SerializeField, Required] private ViewPanel detailsViewPanel;

        [SerializeField, Required] private UniMobSwipeBehaviour swipeBehaviour;
        [SerializeField, Required] private HoldToActionButton   holdToActionButton;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("item_key", () => this.State.ItemKey, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("item_icon", () => this.State.ItemIcon, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("weight", () => this.State.Weight, 1.5f),
            this.Variable("item_rarity", () => this.State.ItemRarity, ERarityType.Common),
            this.Variable("item_cost", () => this.State.ItemCost, Cost.Create(cost => {
                cost.Add(SharedConstants.Game.Currencies.BADGES, 999);
                cost.Add(SharedConstants.Game.Currencies.CRYPT, 90);
            })),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("select", () => this.State.Select()),
        };

        protected override void Activate() {
            base.Activate();

            this.swipeBehaviour.CreateDragAndDropPayloadDelegate = this.State.GetDragAndDropItemPayload;
            
            if (this.holdToActionButton) {
                this.holdToActionButton.fastClick.AddListener(this.State.Select);
                this.holdToActionButton.doubleClick.AddListener(this.State.OnDoubleClick);
            }
        }

        protected override void Deactivate() {
            base.Deactivate();
            
            if (this.holdToActionButton) {
                this.holdToActionButton.fastClick.RemoveListener(this.State.Select);
                this.holdToActionButton.doubleClick.RemoveListener(this.State.OnDoubleClick);
            }

            this.swipeBehaviour.CreateDragAndDropPayloadDelegate = null;
        }

        protected override void Render() {
            base.Render();

            this.swipeBehaviour.enabled = this.State.Draggable;
            
            this.detailsViewPanel.Render(this.State.Details);
        }
    }

    public interface IStorageItemState : IViewState {
        string ItemKey    { get; }
        string ItemIcon    { get; }
        string ItemRarity { get; }

        float Weight { get; }

        Cost ItemCost { get; }

        IState Details { get; }

        bool Draggable { get; }

        DragAndDropPayloadItem GetDragAndDropItemPayload();

        void Select();
        void OnDoubleClick();
    }
}