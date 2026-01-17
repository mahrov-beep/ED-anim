namespace Game.UI.Views.Purchases.Items {
    using Domain;
    using UniMob.UI;
    using Multicast;
    using Multicast.Numerics;

    public class PurchaseKeyIapItemView : AutoView<IPurchaseKeyIapItemState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("purchase_key", () => this.State.PurchaseKey, string.Empty),
            this.Variable("localized_price", () => this.State.LocalizedPrice, "99 USD"),
            this.Variable("amount", () => this.State.Amount),
            this.Variable("is_finite", () => this.State.IsFinite),
            this.Variable("is_sold", () => this.State.IsSold),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("purchase", () => this.State.Purchase()),
        };
    }

    public interface IPurchaseKeyIapItemState : IViewState {
        string    PurchaseKey    { get; }
        string    LocalizedPrice { get; }
        BigDouble Amount         { get; }
        bool      IsSold         { get; }
        bool     IsFinite       { get; }
        void      Purchase();
    }
}