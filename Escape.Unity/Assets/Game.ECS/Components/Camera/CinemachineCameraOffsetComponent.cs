namespace Game.ECS.Components.Camera {
    using System;
    using Scellecs.Morpeh;
    using Unity.Cinemachine;
    using UnityEngine;

    [Serializable, RequireFieldsInit] public struct CinemachineCameraOffsetComponent : IComponent {
        public Vector3 idleOffset;
        public Vector3 movementOffset;

        public float speed;

        public CinemachineCameraOffset cameraOffset;
    }
}