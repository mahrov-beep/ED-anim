namespace Game.UI.Views.Storage {
    using UniMob.UI;
    using Multicast;

    public class StorageItemPartialFillView : AutoView<IStorageItemPartialFillState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("current_parts", () => this.State.CurrentParts, 2),
            this.Variable("required_parts", () => this.State.RequiredParts, 5),
            this.Variable("notify", () => this.State.Notify),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("on_click", () => this.State.OnClick()),
        };
    }

    public interface IStorageItemPartialFillState : IViewState {
        int CurrentParts  { get; }
        int RequiredParts { get; }

        bool Notify { get; }

        void OnClick();
    }
}