namespace Game.UI.Views.Game {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameNearbyItemsView : AutoView<IGameNearbyItemsState> {
        [SerializeField, Required] private ViewPanel itemsView;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("can_equip_best", () => this.State.CanEquipBest, true),
            this.Variable("can_openClose", () => this.State.CanOpenClose, true),
            this.Variable("is_backpack", () => this.State.IsBackpack),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("open-close", () => this.State.OpenClose()),
            this.Event("equip_best", () => this.State.EquipBest()),
        };

        protected override void Render() {
            base.Render();

            this.itemsView.Render(this.State.ItemsState, true);
        }
    }

    public interface IGameNearbyItemsState : IViewState {
        bool CanEquipBest { get; }
        bool CanOpenClose { get; }

        IState ItemsState { get; }

        bool IsBackpack { get; }

        void EquipBest();
        void OpenClose();
    }
}