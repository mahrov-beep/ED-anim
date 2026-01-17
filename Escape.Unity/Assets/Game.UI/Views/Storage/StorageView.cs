namespace Game.UI.Views.Storage {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class StorageView : AutoView<IStorageState> {
        [SerializeField, Required] private ViewPanel               headerPanel;
        [SerializeField, Required] private ViewPanel               itemsPanel;
        [SerializeField, Required] private ViewPanel               filtersPanel;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("show_equip_best_button", () => this.State.ShowEquipBestButton, true),
            this.Variable("show_take_all_button", () => this.State.ShowTakeAllButton, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("equip_best", () => this.State.EquipBest()),
            this.Event("take_all", () => this.State.TakeAll()),
        };

        protected override void Render() {
            base.Render();

            this.headerPanel.Render(this.State.Header);
            this.itemsPanel.Render(this.State.Items, link: true);
            this.filtersPanel.Render(this.State.Filters, link: true);
        }
    }

    public interface IStorageState : IViewState {
        IState Items   { get; }
        IState Header  { get; }
        IState Filters { get; }

        bool ShowTakeAllButton   { get; }
        bool ShowEquipBestButton { get; }

        void TakeAll();
        void EquipBest();
    }
}