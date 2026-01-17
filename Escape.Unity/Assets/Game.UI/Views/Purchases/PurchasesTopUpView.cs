namespace Game.UI.Views.Purchases {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class PurchasesTopUpView : AutoView<IPurchasesTopUpState> {
        [SerializeField, Required] private ViewPanel content;

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };

        protected override void Render() {
            base.Render();
            
            this.content.Render(this.State.ContentState);
        }
    }

    public interface IPurchasesTopUpState : IViewState {
        void   Close();
        IState ContentState { get; }
    }
}