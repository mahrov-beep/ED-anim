namespace Game.UI.Views.Subscription {
    using UniMob.UI;
    using Multicast;

    public class RestoredPurchasesView : AutoView<IRestoredPurchasesState> {
        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("on_close", () => this.State.Close()),
        };
    }

    public interface IRestoredPurchasesState : IViewState {
        void Close();
    }
}