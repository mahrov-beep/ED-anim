namespace Game.UI.Views.GameInventory {
    using Multicast;
    using UniMob.UI;

    public class InventoryItemFilterView : AutoView<IInventoryItemFilterState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("is_right", () => this.State.IsRight && this.State.IsSelected),
            this.Variable("is_left", () => !this.State.IsRight && this.State.IsSelected),
            this.Variable("filter_key", () => this.State.FilterKey),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("on_click", () => this.State.OnClick()),
        };
    }
    
    public interface IInventoryItemFilterState : IViewState {
        public bool IsRight    { get; }
        public bool IsSelected { get; }

        public string FilterKey { get; }

        public void OnClick();
    }
}