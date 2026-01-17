namespace Game.UI.Controllers.Features.Storage {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct StorageFeatureControllerArgs : IFlowControllerArgs {
    }

    public class StorageFeatureController : FlowController<StorageFeatureControllerArgs> {
        [CanBeNull] private IControllerBase storageController;

        protected override async UniTask Activate(Context context) {
            StorageFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenStorage));
        }

        private async UniTask OpenStorage(Context context) {
            if (this.storageController is { IsRunning: true }) {
                Debug.LogError($"[{this}] Storage already opened");
                return;
            }

            this.storageController = await context.RunChild(new StorageControllerArgs());
        }

        [Button]
        private void RaiseOpenStorage() {
            StorageFeatureEvents.Open.Raise();
        }

        [Button]
        private void RaiseCloseStorage() {
            StorageFeatureEvents.Close.Raise();
        }
    }
}