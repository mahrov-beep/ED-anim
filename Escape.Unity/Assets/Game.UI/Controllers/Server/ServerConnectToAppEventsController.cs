namespace Game.UI.Controllers.Server {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain;
    using Multicast;

    [Serializable, RequireFieldsInit]
    public struct ServerConnectToAppEventsControllerArgs : IFlowControllerArgs {
    }

    public class ServerConnectToAppEventsController : FlowController<ServerConnectToAppEventsControllerArgs> {
        [Inject] private SystemModel systemModel;

        protected override async UniTask Activate(Context context) {
            App.Server.ConnectToAppEvents(this.Lifetime, isConnectionLost => this.systemModel.IsInternetConnectionLost = isConnectionLost);
        }
    }
}