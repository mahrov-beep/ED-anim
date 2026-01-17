namespace Game.UI.Views.Items {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class CurrencyItemView : AutoView<ICurrencyItemState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("currency_key", () => this.State.CurrencyKey, SharedConstants.Game.Currencies.BADGES),
            this.Variable("currency_amount", () => this.State.CurrencyAmount, 99999),
        };
    }

    public interface ICurrencyItemState : IViewState {
        string CurrencyKey { get; }

        int CurrencyAmount { get; }
    }
}