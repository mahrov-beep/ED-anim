namespace Game.UI.Views.items {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class ExpItemView : AutoView<IExpItemState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("exp_key", () => this.State.ExpKey, SharedConstants.Game.Exp.MATCH_PLAYED),
            this.Variable("exp_amount", () => this.State.ExpAmount, 899),
        };
    }

    public interface IExpItemState : IViewState {
        string ExpKey { get; }

        int ExpAmount { get; }
    }
}