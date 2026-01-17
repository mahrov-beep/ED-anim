namespace Game.UI.Controllers.Features.GameInventory {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UnityEngine.InputSystem;

    [Serializable, RequireFieldsInit]
    public struct GameInventoryControlsControllerArgs : IFlowControllerArgs {
    }

    public class GameInventoryControlsController : FlowController<GameInventoryControlsControllerArgs> {
        private InputActionMap gameInventoryActionMap;

        protected override async UniTask Activate(Context context) {
            var playerInput = PlayerInput.GetPlayerByIndex(0);
            this.gameInventoryActionMap = playerInput.actions.FindActionMap("GameInventory", true);
            
            this.gameInventoryActionMap.FindAction("CloseInventory", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.CloseInventory, ctx, FlowOptions.NowOrNever));

            var prevActionMap = playerInput.currentActionMap;
            this.Lifetime.Bracket(
                () => playerInput.SwitchCurrentActionMap(this.gameInventoryActionMap.name),
                () => playerInput.SwitchCurrentActionMap(prevActionMap.name)
            );
        }

        private async UniTask CloseInventory(Context context, InputAction.CallbackContext actionContext) {
            GameInventoryFeatureEvents.Close.Raise();
        }
    }
}