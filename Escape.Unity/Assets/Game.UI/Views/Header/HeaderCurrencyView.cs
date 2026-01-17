namespace Game.UI.Views.Header {
    using Multicast;
    using Shared;
    using UniMob.UI;

    public class HeaderCurrencyView : AutoView<IHeaderCurrencyState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("currency_key", () => this.State.CurrencyKey, SharedConstants.Game.Currencies.BADGES),
            this.Variable("amount", () => this.State.Amount, 99900),
            this.Variable("has_add_button", () => this.State.HasAddButton, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("add", () => this.State.Add()),
        };
    }

    public interface IHeaderCurrencyState : IViewState {
        string CurrencyKey { get; }
        int    Amount      { get; }

        bool HasAddButton { get; }

        void Add();
    }
}