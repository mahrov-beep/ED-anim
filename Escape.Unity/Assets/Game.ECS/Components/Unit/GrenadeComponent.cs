namespace Game.ECS.Components.Unit {
    using global::Quantum;
    using UnityEngine;
    using IComponent = Scellecs.Morpeh.IComponent;

    public struct GrenadeComponent : IComponent {
        public EntityRef EntityRef;
        public Vector3 PositionView;
        public QuantumEntityView quantumEntityView;
        public float ExplosionRadius;
    }
}

