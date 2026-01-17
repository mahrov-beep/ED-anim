namespace Game.ECS.Components.Unit {
    using System;
    using Scellecs.Morpeh;
    using UnityEngine.InputSystem;

    [Serializable, RequireFieldsInit] public struct PlayerInputComponent : ISingletonComponent {
        public PlayerInput playerInput;
    }
}