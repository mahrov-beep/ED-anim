namespace Game.UI.Views.ItemInfo {
    using UniMob.UI;
    using Multicast;
    using Multicast.Numerics;
    using Quantum;

    public class ItemInfoStatView : AutoView<IItemInfoStatState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("stat_key", () => this.State.StatKey, EAttributeType.PercentBoost_MoveSpeed),
            this.Variable("stat_value", () => this.State.StatValue, "+99"),
            this.Variable("stat_rarity", () => this.State.StatRarity, ERarityType.Common),
        };
    }

    public interface IItemInfoStatState : IViewState {
        string StatRarity { get; }
        string StatKey    { get; }

        string StatValue { get; }
    }
}