namespace Game.ECS.Providers.Unit {
    using Components.Unit;
    using Scellecs.Morpeh.Providers;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputProvider : MonoProvider<PlayerInputComponent> {
        private void Reset() {
            ref var data = ref GetData();
            data.playerInput = GetComponent<PlayerInput>();
        }
    }
}