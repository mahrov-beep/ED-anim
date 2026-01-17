namespace Game.UI.Views.Purchases {
    using Multicast;
    using UniMob.UI;

    public class PurchasesStoreCategoryView : AutoView<IPurchasesStoreCategoryState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("category_key", () => this.State.StoreCategoryKey),
        };
    }

    public interface IPurchasesStoreCategoryState : IViewState {
        public string StoreCategoryKey { get; }
    }
}