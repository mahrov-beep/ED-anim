namespace Game.ECS.Systems.WorldView {
    using System;
    using Components.WorldView;
    using Scripts;
    using Core;
    using Domain;
    using Game.Services.Photon;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using SoundEffects;
    using UnityEngine.Pool;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public unsafe class HitMarkUiDynamicDataSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LifetimeSystem          lifetimeSystem;
        [Inject] private UiDynamicContext        uiDynamicContext;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;
        [Inject] private LocalPlayerSystem       localPlayerSystem;

        [Inject] private Stash<HitMarkComponent> hitMarkerComponent;

        private Filter healthBarFilter;

        private ObjectPool<HitMarkUiDynamicData>            uiDynamicDataPool;
        private SystemStateProcessor<HitMarkStateComponent> processor;
        
        private IDisposable onUnitDamage;

        private const float DURATION = 0.5f;
        private const float START_FADE_PERCENT = 0.5f;
        
        public override void OnAwake() {
            this.uiDynamicDataPool = new ObjectPool<HitMarkUiDynamicData>(
                () => new HitMarkUiDynamicData(this.lifetimeSystem.SceneLifetime),
                actionOnGet: data => this.uiDynamicContext.Add(data),
                actionOnRelease: data => this.uiDynamicContext.Remove(data));

            this.onUnitDamage = QuantumEvent.SubscribeManual<EventUnitDamage>(this.OnUnitDamage);
            
            this.processor = this.World.Filter
                .With<HitMarkComponent>()
                .ToSystemStateProcessor(this.InitHitMark, this.CleanupHitMark);
        }

        public override void Dispose() {
            this.processor.Dispose();
            this.onUnitDamage.Dispose();

            base.Dispose();
        }

        private void OnUnitDamage(EventUnitDamage callback) {
            if (!this.quantumEntityViewSystem.TryGetEntityView(callback.targetRef, out var view)) {
                return;
            }

            var attack = callback.attack;

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (attack.SourceUnitRef != localRef) {
                return;
            }

            var entity = this.World.CreateEntity();

            ref var hitMarker = ref this.hitMarkerComponent.Add(entity);

            hitMarker.position    = callback.attackPosition.ToUnityVector3();
            hitMarker.damage      = attack.Damage.AsFloat;
            hitMarker.alpha       = 1;
            hitMarker.duration    = DURATION;
            hitMarker.currentTime = DURATION;
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            this.processor.Process();

            foreach (var entity in this.processor.Entities) {
                ref var state  = ref this.processor.States.Get(entity);
                ref var marker = ref this.hitMarkerComponent.Get(entity);

                state.Data.WorldPos = marker.position;
                state.Data.Alpha    = marker.alpha * (marker.currentTime / (marker.duration * START_FADE_PERCENT));
                state.Data.Damage   = marker.damage;

                marker.currentTime -= deltaTime;

                if (marker.currentTime <= 0) {
                    this.hitMarkerComponent.Remove(entity);
                }
            }
        }

        private HitMarkStateComponent InitHitMark(Entity entity) {
            var data = this.uiDynamicDataPool.Get();

            //App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.HIT_MARK);

            return new HitMarkStateComponent {
                Data = data,
            };
        }

        private void CleanupHitMark(ref HitMarkStateComponent state) {
            this.uiDynamicDataPool.Release(state.Data);
        }

        [Serializable, RequireFieldsInit]
        private struct HitMarkStateComponent : ISystemStateComponent {
            public HitMarkUiDynamicData Data;
        }
    }
}