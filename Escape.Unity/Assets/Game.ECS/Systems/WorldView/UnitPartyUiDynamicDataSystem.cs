namespace Game.ECS.Systems.WorldView {
    using System;
    using Components.Unit;
    using Scripts;
    using Core;
    using Multicast;
    using Scellecs.Morpeh;
    using UnityEngine.Pool;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class UnitPartyUiDynamicDataSystem : SystemBase {
        [Inject] private LifetimeSystem   lifetimeSystem;
        [Inject] private UiDynamicContext uiDynamicContext;

        [Inject] private Stash<UnitPartyComponent> partyComponent;

        private Filter unitPartyFilter;

        private ObjectPool<UnitPartyUiDynamicData>            uiDynamicDataPool;
        private SystemStateProcessor<UnitPartyStateComponent> processor;

        public override void OnAwake() {
            this.uiDynamicDataPool = new ObjectPool<UnitPartyUiDynamicData>(
                () => new UnitPartyUiDynamicData(App.Lifetime),
                actionOnGet: data => this.uiDynamicContext.Add(data),
                actionOnRelease: data => this.uiDynamicContext.Remove(data));

            this.processor = this.World.Filter
                .With<UnitPartyComponent>()
                .ToSystemStateProcessor(this.IntPartyUnit, this.CleanupParty);
        }

        public override void Dispose() {
            this.processor.Dispose();
            this.uiDynamicDataPool.Dispose();

            base.Dispose();
        }

        public override void OnUpdate(float deltaTime) {
            this.processor.Process();

            foreach (var entity in this.processor.Entities) {
                ref var party   = ref this.partyComponent.Get(entity);
                ref var state  = ref this.processor.States.Get(entity);

                if (state.Data == null) {
                    continue;
                }
                
                state.Data.WorldPos = party.transform.position;
                state.Data.NickName = party.nickName;
                state.Data.Level    = party.level;
                state.Data.Guid     = party.guid;
            }
        }

        private UnitPartyStateComponent IntPartyUnit(Entity entity) {
            var data = this.uiDynamicDataPool.Get();

            return new UnitPartyStateComponent {
                Data = data,
            };
        }

        private void CleanupParty(ref UnitPartyStateComponent state) {
            this.uiDynamicDataPool.Release(state.Data);
        }
        

        [Serializable, RequireFieldsInit]
        private struct UnitPartyStateComponent : ISystemStateComponent {
            public UnitPartyUiDynamicData Data;
        }
    }
}