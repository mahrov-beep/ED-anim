namespace Game.ECS.Systems.Unit {
    using System;
    using Camera;
    using Components.Camera;
    using Components.Unit;
    using Core;
    using Domain.Game;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using UnityEngine;
    using Utilities;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class UpdateDamageCueModelSystem : SystemBase {
        [Inject] private LocalPlayerSystem                 localPlayerSystem;
        [Inject] private Stash<UnitComponent>              unitStash;
        [Inject] private Stash<VisiblyInFrustumMarker>     visibilityStash;
        [Inject] private Stash<InLocalCameraFrustumMarker> inFrustumStash;
        [Inject] private QuantumEntityViewSystem           updater;
        [Inject] private MapperUnitEntityRefToEntitySystem mapperEntityRefToEntity;
        [Inject] private DamageSourceModel                 model;
        [Inject] private CueUIVisualizeConfig              config;

        private SingletonFilter<CinemachineBrainComponent> cameraFilter;

        private IDisposable subscribe;

        public override void OnAwake() {
            cameraFilter = World.Filter.Singleton<CinemachineBrainComponent>();
            subscribe    = QuantumEvent.SubscribeManual<EventUnitDamage>(OnUnitDamage);
        }

        public override void Dispose() {
            subscribe.Dispose();
        }

        public override void OnUpdate(float deltaTime) {
            var localRef = localPlayerSystem.LocalEntity;
            ref var cameraBrain = ref cameraFilter.Instance;

            for (var i = model.DamageSourceScreenNormalizedDirections.Count - 1; i >= 0; i--) {
                var data = model.DamageSourceScreenNormalizedDirections[i];

                if (mapperEntityRefToEntity.TryGet(data.SourceEntityRef, out var sourceEntity)) {
                    if (!localRef.IsNullOrDisposed() && unitStash.Has(localRef) && unitStash.Has(sourceEntity)) {
                        ref var localUnitComponent  = ref unitStash.Get(localRef);
                        ref var sourceUnitComponent = ref unitStash.Get(sourceEntity);

                        data.WorldPosition = sourceUnitComponent.PositionView;
                        data.ScreenPosition = cameraBrain.camera.WorldToScreenPoint(data.WorldPosition);
                        data.IsOnScreen = ScreenSpaceHelper.IsPositionOnScreen(data.ScreenPosition) && inFrustumStash.Has(sourceEntity);

                        var direction = sourceUnitComponent.PositionView - localUnitComponent.PositionView;

                        if (ScreenSpaceHelper.TryGetScreenDirection(direction, cameraBrain.Transform, out var newScreenDir)) {
                            data.ScreenNormalizedDirection = newScreenDir;
                        }
                    }
                }

                if (data.Timer.ProcessTimer(deltaTime)) {
                    model.DamageSourceScreenNormalizedDirections.RemoveAt(i);
                }
                else {
                    model.DamageSourceScreenNormalizedDirections[i] = data;
                }
            }
        }

        private void OnUnitDamage(EventUnitDamage callback) {
            if (callback.attack.DamageType == EDamageType.Fire || callback.attack.DamageType == EDamageType.DamageZone) {
                return; // нет направления, горит пукан
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (callback.targetRef != localRef) {
                return;
            }

            var attack        = callback.attack;
            var sourceUnitRef = attack.SourceUnitRef;

            if (sourceUnitRef == localRef) {
                return;
            }

            if (!updater.TryGetEntityView(sourceUnitRef, out var attackerView)) {
                return;
            }

            if (!updater.TryGetEntityView(localRef, out var localView)) {
                return;
            }

            var sourcePosition = attack.DamageType switch {
                            EDamageType.Explosion => callback.attackPosition.ToUnityVector3(),
                            _ => attackerView.Transform.position,
            };

            var direction = sourcePosition - localView.Transform.position;
            
            ref var cameraBrain = ref cameraFilter.Instance;

            if (!ScreenSpaceHelper.TryGetScreenDirection(direction, cameraBrain.Transform, out var screenDirection)) {
                return; // Direction too small or degenerate
            }

            var screenPos = cameraBrain.camera.WorldToScreenPoint(sourcePosition);
            var isOnScreen = false;

            if (mapperEntityRefToEntity.TryGet(sourceUnitRef, out var sourceEntity)) {
                isOnScreen = ScreenSpaceHelper.IsPositionOnScreen(screenPos) && inFrustumStash.Has(sourceEntity);
            }

            for (var i = 0; i < model.DamageSourceScreenNormalizedDirections.Count; i++) {
                if (model.DamageSourceScreenNormalizedDirections[i].SourceEntityRef == sourceUnitRef) {
                    var existing = model.DamageSourceScreenNormalizedDirections[i];
                    existing.Timer                     = config.damageMarkerLifetime;
                    existing.ScreenNormalizedDirection = screenDirection;
                    existing.WorldPosition             = sourcePosition;
                    existing.ScreenPosition            = screenPos;
                    existing.IsOnScreen                = isOnScreen;
                    model.DamageSourceScreenNormalizedDirections[i] = existing;
                    return;
                }
            }

            var damageSource = new DamageSourceData {
                            ScreenNormalizedDirection = screenDirection,
                            Timer                     = config.damageMarkerLifetime,
                            SourceEntityRef           = sourceUnitRef,
                            WorldPosition             = sourcePosition,
                            ScreenPosition            = screenPos,
                            IsOnScreen                = isOnScreen,
            };

            model.DamageSourceScreenNormalizedDirections.Add(damageSource);
        }
    }
}