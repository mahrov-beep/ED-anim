namespace Game.UI.Controllers.Features.Gunsmith {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct GunsmithFeatureControllerArgs : IFlowControllerArgs {
    }

    public class GunsmithFeatureController : FlowController<GunsmithFeatureControllerArgs> {
        [CanBeNull] private IControllerBase gunsmithController;

        protected override async UniTask Activate(Context context) {
            GunsmithFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenGunsmith));
        }

        private async UniTask OpenGunsmith(Context context) {
            if (this.gunsmithController is { IsRunning: true }) {
                Debug.LogError($"[{this}] Gunsmith already opened");
                return;
            }

            this.gunsmithController = await context.RunChild(new GunsmithControllerArgs());
        }

        [Button]
        private void RaiseOpenGunsmith() {
            GunsmithFeatureEvents.Open.Raise();
        }

        [Button]
        private void RaiseCloseGunsmith() {
            GunsmithFeatureEvents.Close.Raise();
        }
    }
}