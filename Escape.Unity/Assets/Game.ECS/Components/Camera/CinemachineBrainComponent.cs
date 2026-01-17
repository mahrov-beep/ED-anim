namespace Game.ECS.Components.Camera {
    using System;
    using Unity.Cinemachine;
    using Scellecs.Morpeh;
    using UnityEngine;

    [Serializable, RequireFieldsInit] public struct CinemachineBrainComponent : ISingletonComponent {
        public CinemachineBrain brain;
        public Camera           camera;
        public Transform        Transform;
    }
}