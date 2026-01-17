namespace Game.ECS.Systems.Unit {

    using Components.Unit;
    using Multicast;
    using Scellecs.Morpeh;

    public class EventUnitRebirthHandler : QuantumEventHandlerSystem<Quantum.EventUnitRebirth> {
        [Inject] MapperUnitEntityRefToEntitySystem mapperUnitEntityRefToEntitySystem;

        [Inject] Stash<UnitComponent>  stashUnits;
        [Inject] Stash<UnitDeadMarker> stashDeadUnits;

        protected override void OnReceive(Quantum.EventUnitRebirth data) {
            if (!mapperUnitEntityRefToEntitySystem.TryGet(data.unitRef, value: out var entity)) {
                return;
            }

            if (!stashUnits.Has(entity)) {
                return;
            }

            ref var unit = ref stashUnits.Get(entity);

            if (!unit.AimAssistTarget) {
                unit.AimAssistTarget.SetActive(true);
            }

            if (stashDeadUnits.Has(entity)) {
                stashDeadUnits.Remove(entity);
            }
        }
    }
}