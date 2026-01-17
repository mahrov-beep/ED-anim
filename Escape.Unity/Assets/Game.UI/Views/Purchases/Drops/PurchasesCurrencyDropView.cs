namespace Game.UI.Views.Purchases.Drops {
    using Multicast;
    using Multicast.Numerics;
    using UniMob.UI;

    public class PurchasesCurrencyDropView : AutoView<IPurchasesCurrencyDropState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("category_key", () => this.State.CategoryKey),
            this.Variable("purchase_key", () => this.State.PurchaseKey),
            this.Variable("currency_key", () => this.State.CurrencyKey),
            this.Variable("amount", () => this.State.Amount),
        };
    }

    public interface IPurchasesCurrencyDropState : IViewState {
        string    CategoryKey { get; }
        string    PurchaseKey { get; }
        string    CurrencyKey { get; }
        BigDouble Amount      { get; }
    }
}