namespace Game.UI.Views.Store {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class StoreItemView : AutoView<IStoreItemState> {
        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("buy", () => this.State.Buy()),
        };
    }

    public interface IStoreItemState : IViewState {
        void Buy();
    }
}