namespace Game.UI.Controllers.Features.GameplayStart {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Game.Domain.Party;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct GameplayStartFeatureControllerArgs : IFlowControllerArgs {
        public IScenesController ScenesController;
    }

    public class GameplayStartFeatureController : FlowController<GameplayStartFeatureControllerArgs> {
        private IDisposableController startController;
        [Inject] private PartyModel partyModel;

        protected override async UniTask Activate(Context context) {
            GameplayStartFeatureEvents.Start.Listen(this.Lifetime, () => this.RequestFlow(this.Start));
        }

        private async UniTask Start(Context context) {
            if (this.startController is { IsRunning: true }) {
                return;
            }

            var members = this.partyModel.Members.Value;
            if (members != null && members.Length > 1) {
                if (!this.partyModel.IsLeader) {
                    Debug.LogWarning("MM Start blocked: not a leader");
                    return;
                }
                if (!this.partyModel.AreAllReady) {
                    Debug.LogWarning("MM Start blocked: not all ready");
                    return;
                }
            }

            this.startController = await context.RunDisposable(new GameplayStartSequenceControllerArgs {
                ScenesController = this.Args.ScenesController,
            });
        }
    }
}