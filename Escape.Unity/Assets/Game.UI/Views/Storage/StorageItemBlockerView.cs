namespace Game.UI.Views.Storage {
    using UniMob.UI;
    using Multicast;

    public class StorageItemBlockerView : AutoView<IStorageItemBlockerState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("blocker_text", () => this.State.BlockerText, "Sample blocker text"),
        };
    }

    public interface IStorageItemBlockerState : IViewState {
        string BlockerText { get; }
    }
}