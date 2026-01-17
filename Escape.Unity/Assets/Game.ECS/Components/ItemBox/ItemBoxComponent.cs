namespace Game.ECS.Components.ItemBox {
    using System;
    using global::Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using IComponent = Scellecs.Morpeh.IComponent;

    [Serializable]
    public struct ItemBoxComponent : IComponent {
        [Required] public QuantumEntityView quantumEntityView;

        [HideInInspector] public float timer;
        
        [HideInInspector] public float time;
        
        [HideInInspector] public Vector3 position;
    }

    [Serializable]
    public struct ItemBoxTimerMarker : IComponent {
        
    }
}