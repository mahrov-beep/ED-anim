namespace Game.ECS.Systems.Core {
    using Components.Quantum;
    using JetBrains.Annotations;
    using Quantum;
    using Scellecs.Morpeh;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class QuantumEntityViewSystem : SystemBase {
        private SingletonFilter<QuantumEntityViewUpdaterComponent> quantumViewUpdaterSingleton;

        public override void OnAwake() {
            this.quantumViewUpdaterSingleton = this.World.FilterSingleton<QuantumEntityViewUpdaterComponent>();
        }

        public override void OnUpdate(float deltaTime) {
        }

        [PublicAPI]
        public bool TryGetEntityView(EntityRef entityRef, out QuantumEntityView view) {
            var entityViewUpdater = this.quantumViewUpdaterSingleton.Instance.updater;
            view = entityViewUpdater.GetView(entityRef);
            return view != null;
        }
    }
}