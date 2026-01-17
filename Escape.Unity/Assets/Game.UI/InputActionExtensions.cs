namespace Game.UI {
    using System;
    using JetBrains.Annotations;
    using Multicast;
    using UniMob;
    using UnityEngine.InputSystem;

    public static class InputActionExtensions {
        [PublicAPI]
        public static void Subscribe(this InputAction inputAction, Lifetime lifetime, Action<InputAction.CallbackContext> callback) {
            if (lifetime.IsDisposed) {
                return;
            }

            lifetime.Register(() => inputAction.performed -= callback);
            inputAction.performed += callback;
        }
    }
}