namespace Game.UI.Views.Purchases.Items {
    using Domain;
    using Multicast;
    using Multicast.Numerics;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class PurchasesCurrencyItemView : AutoView<IPurchasesCurrencyItemState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("category_key", () => this.State.CategoryKey),
            this.Variable("purchase_key", () => this.State.PurchaseKey),
            this.Variable("price_currency_key", () => this.State.PriceCurrency),
            this.Variable("reward_currency_key", () => this.State.RewardCurrency),
            this.Variable("reward", () => this.State.Reward, 0),
            this.Variable("price", () => this.State.Price, 100),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("buy", () => this.State.StartPurchase()),
        };
    }

    public interface IPurchasesCurrencyItemState : IViewState {
        public string    CategoryKey    { get; }
        public string    PurchaseKey    { get; }
        public string    PriceCurrency  { get; }
        public string    RewardCurrency { get; }
        public BigDouble Price          { get; }
        public BigDouble Reward         { get; }

        void StartPurchase();
    }
}