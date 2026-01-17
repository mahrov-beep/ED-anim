namespace Game.UI.Views.items {
    using UniMob.UI;
    using Multicast;
    using Quantum;

    public class ItemGroupingSeparatorView : AutoView<IItemGroupingSeparatorState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("grouping", () => this.State.Grouping, ItemAssetGrouping.Other),
        };
    }

    public interface IItemGroupingSeparatorState : IViewState {
        string Grouping { get; }
    }
}