namespace Game.UI.Views.RewardLarge {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class RewardLargeItemView : AutoView<IRewardLargeItemState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("item_key", () => this.State.ItemKey, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("item_icon", () => this.State.ItemIcon, SharedConstants.Game.Items.WEAPON_AR),
        };
    }

    public interface IRewardLargeItemState : IViewState {
        string ItemKey  { get; }
        string ItemIcon { get; }
    }
}