namespace Game.UI.Views.Notifier {
    using UniMob.UI;
    using Multicast;

    public class NotifierView : AutoView<INotifierState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("notifier", () => this.State.Notifier, 9),
            this.Variable("is_new", () => this.State.IsNew),
        };
    }

    public interface INotifierState : IViewState {
        int  Notifier { get; }
        bool IsNew    { get; }
    }
}