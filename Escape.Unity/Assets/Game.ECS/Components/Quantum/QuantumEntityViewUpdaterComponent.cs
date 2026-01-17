namespace Game.ECS.Components.Quantum {
using System;
using global::Quantum;
using Scellecs.Morpeh;

[Serializable, RequireFieldsInit] public struct QuantumEntityViewUpdaterComponent : ISingletonComponent {
    public QuantumEntityViewUpdater updater;
}
}