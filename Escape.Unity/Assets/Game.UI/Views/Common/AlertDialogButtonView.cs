namespace Game.UI.Views.Common {
    using UniMob.UI;
    using Multicast;

    public class AlertDialogButtonView : AutoView<IAlertDialogButtonState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("button_key", () => this.State.ButtonKey),
            this.Variable("is_interactable", () => this.State.IsInteractable),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("click", () => this.State.Click()),
        };
    }

    public interface IAlertDialogButtonState : IViewState {
        void   Click();
        string ButtonKey      { get; }
        bool   IsInteractable { get; }
    }
}