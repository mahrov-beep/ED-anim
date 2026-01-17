namespace Game.UI.Controllers.Features.GameInventory {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;

    [Serializable, RequireFieldsInit]
    public struct GameInventoryFeatureControllerArgs : IFlowControllerArgs {
    }

    public class GameInventoryFeatureController : FlowController<GameInventoryFeatureControllerArgs> {
        private IControllerBase gameInventory;
        
        protected override async UniTask Activate(Context context) {
            GameInventoryFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenGameInventory));
        }

        private async UniTask OpenGameInventory(Context context) {
            if (this.gameInventory is { Lifetime: { IsDisposed: false } }) {
                return;
            }

            this.gameInventory = await context.RunChild(new GameInventoryScreenControllerArgs());
        }
    }
}