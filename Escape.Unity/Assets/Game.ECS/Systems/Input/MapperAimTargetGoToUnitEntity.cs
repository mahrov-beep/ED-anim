namespace Game.ECS.Systems.Input {
    using System.Collections.Generic;
    using Components.Unit;
    using Multicast;
    using Scellecs.Morpeh;
    using Unit;
    using UnityEngine;

    public class MapperAimTargetGoToUnitEntity : SystemBase, IDictionaryProvider<GameObject, Entity> {
        private Filter filterUnitEntity;

        [Inject] private Stash<UnitComponent> stashUnit;

        public Dictionary<GameObject, Entity> Dictionary { get; } = new();

        public override void Dispose() {
            Dictionary.Clear();
        }

        public override void OnAwake() {
            filterUnitEntity = World.Filter.With<UnitComponent>().Build();
        }

        public override void OnUpdate(float deltaTime) {
            Dictionary.Clear();

            foreach (var entity in filterUnitEntity) {
                ref var unit = ref stashUnit.Get(entity);
                if (!unit.AimAssistTarget) {
                    continue;
                }

                Dictionary[unit.AimAssistTarget] = entity;
            }
        }
    }
}