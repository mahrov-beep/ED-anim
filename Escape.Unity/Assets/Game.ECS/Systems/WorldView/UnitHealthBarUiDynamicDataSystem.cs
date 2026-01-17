namespace Game.ECS.Systems.WorldView {
    using System;
    using System.Collections.Generic;
    using Components.Unit;
    using Scripts;
    using Core;
    using Game.Services.Photon;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using Shared.UserProfile.Data;
    using UnityEngine;
    using UnityEngine.Pool;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public unsafe class UnitHealthBarUiDynamicDataSystem : SystemBase {
        [Inject] private PhotonService    photonService;
        [Inject] private LifetimeSystem   lifetimeSystem;
        [Inject] private UiDynamicContext uiDynamicContext;

        [Inject] private Stash<UnitComponent>       unitComponent;
        [Inject] private Stash<UnitHealthBarMarker> unitHealthBarMarker;

        private Filter healthBarFilter;

        private ObjectPool<UnitHealthBarUiDynamicData>            uiDynamicDataPool;
        private SystemStateProcessor<UnitHealthBarStateComponent> processor;

        private readonly Dictionary<QString32, string> nickNamesCache = new Dictionary<QString32, string>();

        public override void OnAwake() {
            this.uiDynamicDataPool = new ObjectPool<UnitHealthBarUiDynamicData>(
                () => new UnitHealthBarUiDynamicData(this.lifetimeSystem.SceneLifetime),
                actionOnGet: data => this.uiDynamicContext.Add(data),
                actionOnRelease: data => this.uiDynamicContext.Remove(data));

            this.processor = this.World.Filter
                .With<UnitComponent>()
                .With<UnitHealthBarMarker>()
                //.Without<LocalCharacterMarker>()
                .ToSystemStateProcessor(this.InitHealthBar, this.CleanupHealthBar);
        }

        public override void Dispose() {
            this.processor.Dispose();

            base.Dispose();
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            this.processor.Process();

            foreach (var entity in this.processor.Entities) {
                ref var unit   = ref this.unitComponent.Get(entity);
                ref var state  = ref this.processor.States.Get(entity);
                ref var marker = ref this.unitHealthBarMarker.Get(entity);

                var unitTransform = unit.quantumEntityView.transform;

                var unitEntityRef = unit.EntityRef;

                if (!f.TryGet(unitEntityRef, out Health health)) {
                    continue;
                }

                state.Data.Health    = health.CurrentValue.AsFloat;
                state.Data.MaxHealth = health.MaxValue.AsFloat;
                state.Data.WorldPos  = unitTransform.position + unit.healthBarOffset;
                state.Data.IsDead    = CharacterFsm.CurrentStateIs<CharacterStateDead>(f, unitEntityRef);
                state.Data.NickName  = this.GetNickName(f, unitEntityRef);
                state.Data.Alpha     = marker.alpha;

                state.Data.IsKnocked       = CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, unitEntityRef);
                state.Data.IsBeingRevived  = false;
                state.Data.KnockHealth     = 0f;

                if (state.Data.IsKnocked && f.TryGetPointer(unitEntityRef, out CharacterStateKnocked* knocked)) {
                    state.Data.IsBeingRevived = knocked->IsBeingRevived;
                    state.Data.KnockHealth    = knocked->KnockHealth.AsFloat;
                }
            }
        }

        private UnitHealthBarStateComponent InitHealthBar(Entity entity) {
            var data = this.uiDynamicDataPool.Get();

            return new UnitHealthBarStateComponent {
                Data = data,
            };
        }

        private void CleanupHealthBar(ref UnitHealthBarStateComponent state) {
            this.uiDynamicDataPool.Release(state.Data);
        }

        private string GetNickName(Frame f, EntityRef unitEntityRef) {
            if (f.TryGet(unitEntityRef, out Unit unit) && f.GetPlayerData(unit.PlayerRef) is { } playerData) {
                return playerData.NickName;
            }

            if (f.TryGet(unitEntityRef, out NickName nickName)) {
                if (this.nickNamesCache.TryGetValue(nickName.Value, out var cacheNickName)) {
                    return cacheNickName;
                }

                return this.nickNamesCache[nickName.Value] = nickName.Value.ToString();
            }

            return "";
        }

        [Serializable, RequireFieldsInit]
        private struct UnitHealthBarStateComponent : ISystemStateComponent {
            public UnitHealthBarUiDynamicData Data;
        }
    }
}