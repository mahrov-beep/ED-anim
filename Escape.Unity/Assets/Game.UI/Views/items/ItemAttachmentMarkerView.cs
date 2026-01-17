namespace Game.UI.Views.items {
    using UniMob.UI;
    using Multicast;
    using Quantum;

    public class ItemAttachmentMarkerView : AutoView<IItemAttachmentMarkerState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("item_rarity", () => this.State.ItemRarity, ERarityType.Common),
        };
    }

    public interface IItemAttachmentMarkerState : IViewState {
        string ItemRarity { get; }
    }
}