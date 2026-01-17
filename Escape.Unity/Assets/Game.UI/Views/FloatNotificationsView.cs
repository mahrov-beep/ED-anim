namespace Game.UI.Views {
    using UniMob.UI;
    using Multicast;

    public class FloatNotificationsView : AutoView<IFloatNotificationsState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("no_internet", () => this.State.NoInternetConnection, true),
        };
    }

    public interface IFloatNotificationsState : IViewState {
        bool NoInternetConnection { get; }
    }
}