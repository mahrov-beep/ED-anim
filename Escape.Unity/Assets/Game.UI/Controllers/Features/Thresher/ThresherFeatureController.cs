namespace Game.UI.Controllers.Features.Thresher {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct ThresherFeatureControllerArgs : IFlowControllerArgs {
    }

    public class ThresherFeatureController : FlowController<ThresherFeatureControllerArgs> {
        [CanBeNull] private IControllerBase thresherController;

        protected override async UniTask Activate(Context context) {
            ThresherFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenThresher));
        }

        private async UniTask OpenThresher(Context context) {
            if (this.thresherController is { IsRunning: true }) {
                Debug.LogError($"[{this}] Thresher already opened");
                return;
            }

            this.thresherController = await context.RunChild(new ThresherControllerArgs());
        }

        [Button]
        private void RaiseOpenThresher() {
            ThresherFeatureEvents.Open.Raise();
        }

        [Button]
        private void RaiseCloseThresher() {
            ThresherFeatureEvents.Close.Raise();
        }
    }
}