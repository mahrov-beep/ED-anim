namespace Game.UI.Views.GameInventory {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameInventoryTetrisView : AutoView<IGameInventoryTetrisState> {
        [SerializeField, Required] private ViewPanel cells;
        [SerializeField, Required] private ViewPanel tetris;
        
        protected override void Render() {
            base.Render();

            this.cells.Render(this.State.Cells);
            this.tetris.Render(this.State.Tetris);
        }
    }

    public interface IGameInventoryTetrisState : IViewState {
        IState Cells { get; }
        IState Tetris { get; }
    }
}