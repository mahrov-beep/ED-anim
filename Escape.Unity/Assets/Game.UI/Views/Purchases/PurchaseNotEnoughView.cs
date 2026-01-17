namespace Game.UI.Views.Purchases {
    using UniMob.UI;
    using Multicast;

    public class PurchaseNotEnoughView : AutoView<IPurchaseNotEnoughState> {
        public void Close() {
            if (this.HasState) {
                this.State.Close();
            }
        }
    }

    public interface IPurchaseNotEnoughState : IViewState {
        void Close();
    }
}