namespace Game.ECS.Providers.Quantum {
using System;
using Components.Quantum;
using global::Quantum;
using Scellecs.Morpeh.Providers;

public class QuantumEntityViewUpdaterProvider : MonoProvider<QuantumEntityViewUpdaterComponent> {
    private void Reset() {
        GetData().updater = GetComponent<QuantumEntityViewUpdater>();
    }
}
}