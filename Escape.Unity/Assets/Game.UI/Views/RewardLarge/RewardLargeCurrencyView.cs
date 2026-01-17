namespace Game.UI.Views.RewardLarge {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class RewardLargeCurrencyView : AutoView<IRewardLargeCurrencyState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("currency_key", () => this.State.CurrencyKey, SharedConstants.Game.Currencies.BUCKS),
            this.Variable("currency_icon", () => this.State.CurrencyKey, SharedConstants.Game.Currencies.BUCKS),
        };
    }

    public interface IRewardLargeCurrencyState : IViewState {
        string CurrencyKey { get; }
    }
}