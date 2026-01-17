namespace Game.UI.Controllers.Server {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Shared.ServerEvents;
    using UnityEngine;

    public class ServerListenDebugLogEventController : FlowController<ServerListenDebugLogEventControllerArgs> {
        protected override async UniTask Activate(Context context) {
            App.Events.Listen<DebugLogAppServerEvent>(this.Lifetime, this.OnDebugLogEvent);
        }

        private void OnDebugLogEvent(DebugLogAppServerEvent evt) {
            Debug.Log(evt.Message);
        }
    }

    [Serializable, RequireFieldsInit]
    public struct ServerListenDebugLogEventControllerArgs : IFlowControllerArgs {
    }
}