namespace Game.UI.Views.GameInventory {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameInventoryView : AutoView<IGameInventoryState> {
        [SerializeField, Required] private ViewPanel slotItemsPanel;
        [SerializeField, Required] private ViewPanel trashItemsPanel;
        [SerializeField, Required] private ViewPanel filtersPanel;
        [SerializeField, Required] private ViewPanel nearbyItemsPanel;

        [SerializeField, Required] private UniMobDropZoneBehaviour throwDropZone;

        protected override void Awake() {
            base.Awake();

            this.throwDropZone.IsPayloadAcceptableDelegate = p => {
                if (!this.HasState) {
                    return false;
                }

                return p is DragAndDropPayloadItem itemEntity && this.State.CanThrowItem(itemEntity);
            };
            this.throwDropZone.OnAccept.AddListener(p => {
                if (this.HasState && p is DragAndDropPayloadItem payloadItem) {
                    this.State.OnThrowItem(payloadItem);
                }
            });
        }

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("current_items_weight", () => this.State.CurrentItemsWeight, 80),
            this.Variable("limit_items_weight", () => this.State.LimitItemsWeight, 100),
            this.Variable("show_item_throw_zone", () => this.State.ShowItemsThrowZone, true),
            this.Variable("loadout_quality", () => this.State.LoadoutQuality, 999),
            this.Variable("has_some_loadouts", () => this.State.HasSomeLoadouts, true),
            this.Variable("current_loadout_index", () => this.State.CurrentLoadoutIndex, 1),
            this.Variable("loadout_count", () => this.State.LoadoutCount, 2),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
            this.Event("on_increment_loadout", () => this.State.OnIncrementLoadout()),
            this.Event("on_decrement_loadout", () => this.State.OnDecrementLoadout()),
        };

        protected override void Render() {
            base.Render();

            this.slotItemsPanel.Render(this.State.SlotItems, true);
            this.trashItemsPanel.Render(this.State.TrashItems, true);
            this.filtersPanel.Render(this.State.Filters, true);
            this.nearbyItemsPanel.Render(this.State.NearbyItems, true);
        }
    }

    public interface IGameInventoryState : IViewState {
        float CurrentItemsWeight  { get; }
        float LimitItemsWeight    { get; }
        int   LoadoutQuality      { get; }
        int   CurrentLoadoutIndex { get; }
        int   LoadoutCount        { get; }

        bool ShowItemsThrowZone { get; }
        bool HasSomeLoadouts    { get; }

        IState SlotItems   { get; }
        IState TrashItems  { get; }
        IState Filters     { get; }
        IState NearbyItems { get; }

        void Close();
        void OnIncrementLoadout();
        void OnDecrementLoadout();
        
        bool CanThrowItem(DragAndDropPayloadItem payload);
        void OnThrowItem(DragAndDropPayloadItem payload);
    }
}