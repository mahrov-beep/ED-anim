namespace Game.ECS.Systems.Unit {

    using Components.Unit;
    using Multicast;
    using Scellecs.Morpeh;

    public class EventUnitDeadHandler : QuantumEventHandlerSystem<Quantum.EventUnitDead> {
        [Inject] MapperUnitEntityRefToEntitySystem mapperUnitEntityRefToEntitySystem;

        [Inject] Stash<UnitComponent>  stashUnits;
        [Inject] Stash<UnitDeadMarker> stashDeadUnits;

        protected override void OnReceive(Quantum.EventUnitDead data) {
            if (!mapperUnitEntityRefToEntitySystem.TryGet(data.unitRef, value: out var entity)) {
                return;
            }

            if (!stashUnits.Has(entity)) {
                return;
            }

            ref var unit = ref stashUnits.Get(entity);

            unit.AimAssistTarget.SetActive(false);

            stashDeadUnits.Set(entity, new UnitDeadMarker());
        }
    }
}