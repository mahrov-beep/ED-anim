namespace Game.UI.Views.Purchases.Items {
    using Domain;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class PurchasesIapItemView : AutoView<IPurchasesIapItemState> {
        [SerializeField, Required] private ViewPanel dropPanel;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("category_key", () => this.State.CategoryKey),
            this.Variable("purchase_key", () => this.State.PurchaseKey, string.Empty),
            this.Variable("localized_price", () => this.State.LocalizedPrice, "99 USD"),
            this.Variable("has_new_mention", () => this.State.HasNewMention, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("buy", () => this.State.Purchase()),
        };

        protected override void Render() {
            base.Render();
            this.dropPanel.Render(this.State.DropState);
        }
    }

    public interface IPurchasesIapItemState : IViewState {
        public string CategoryKey    { get; }
        public string PurchaseKey    { get; }
        public string LocalizedPrice { get; }

        public bool HasNewMention { get; }

        public IState DropState { get; }

        void Purchase();
    }
}