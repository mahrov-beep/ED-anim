namespace Game.ECS.Systems.Unit {
    using System.Collections.Generic;
    using Components.Unit;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using Input = Quantum.Input;
    using SystemBase = Scellecs.Morpeh.SystemBase;
    public class MapperUnitEntityRefToEntitySystem : SystemBase,
                    IDictionaryProvider<EntityRef, Entity> {

        public Dictionary<EntityRef, Entity> Dictionary { get; } = new();
        
        [Inject] private Stash<UnitComponent> unitsStash;
        
        [Inject] private PhotonService        photonService;

        private Filter notLocalUnits;

        public override void OnAwake() {
            notLocalUnits = World.Filter
                            .With<UnitComponent>()
                            .Build();
        }

        public override void Dispose() {
            Dictionary.Clear();
        }

        public override void OnUpdate(float deltaTime) {
            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            // мб можно без Clear, у нас персонажи имеют BindBehavior == Verified,
            // значит можно просто на событиях обновлять
            Dictionary.Clear();

            foreach (var notLocalUnit in notLocalUnits) {
                ref var unit = ref unitsStash.Get(notLocalUnit);

                Dictionary[unit.EntityRef] = notLocalUnit;
            }
        }
    }
}