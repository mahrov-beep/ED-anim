namespace Game.ECS.Components.Camera {
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Scellecs.Morpeh;
using UnityEngine;
[Serializable, RequireFieldsInit] public struct CinemachineVirtualCameraComponent : IComponent {
    public CinemachineCamera                      camera;
    
    [NonSerialized]
    public Dictionary<Type, CinemachineExtension> extensionsMap;
}
}