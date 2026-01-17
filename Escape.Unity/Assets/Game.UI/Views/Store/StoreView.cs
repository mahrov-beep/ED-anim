namespace Game.UI.Views.Store {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class StoreView : AutoView<IStoreState> {
        [SerializeField, Required] private ViewPanel header;
        [SerializeField, Required] private ViewPanel itemsPanel;

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };

        protected override void Render() {
            base.Render();
            
            this.header.Render(this.State.Header);
            this.itemsPanel.Render(this.State.PurchasesWidget, link: true);
        }
    }

    public interface IStoreState : IViewState {
        IState Header          { get; }
        IState PurchasesWidget { get; }

        void Close();
    }
}