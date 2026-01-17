namespace Game.UI.Controllers.Server {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Shared.ServerEvents;
    using Shared.UserProfile.Commands;
    using UnityEngine;

    public class ServerListenProfileUpdatedEventController : FlowController<ServerListenProfileUpdatedEventControllerArgs> {
        private bool reFetchRequired;

        protected override async UniTask Activate(Context context) {
            App.Events.Listen<UserProfileUpdatedAppServerEvent>(this.Lifetime, evt => {
                this.reFetchRequired = true;
                App.RequestAppUpdateFlow();
            });
        }

        protected override async UniTask OnFlow(Context context) {
            if (!this.reFetchRequired) {
                return;
            }

            this.reFetchRequired = false;

            try {
                await context.Server.ExecuteUserProfile(new UserProfileFetchCommand(), ServerCallRetryStrategy.Throw);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
    }

    [Serializable, RequireFieldsInit]
    public struct ServerListenProfileUpdatedEventControllerArgs : IFlowControllerArgs {
    }
}