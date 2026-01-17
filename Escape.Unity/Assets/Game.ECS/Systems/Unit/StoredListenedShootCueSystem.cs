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
    using Services.Photon;
    using UnityEngine;
    using Utilities;
    using SystemBase = Scellecs.Morpeh.SystemBase;
    public class StoredListenedShootCueSystem : SystemBase {
        [Inject] private PhotonService photonService;

        [Inject] private ListenedCueModel model;

        [Inject] private LocalPlayerSystem                 localPlayerSystem;
        [Inject] private QuantumEntityViewSystem           updater;
        [Inject] private MapperUnitEntityRefToEntitySystem mapperEntityRefToEntity;

        [Inject] private Stash<VisiblyInFrustumMarker>     visibilityStash;
        [Inject] private Stash<InLocalCameraFrustumMarker> inFrustumStash;
        [Inject] private Stash<UnitComponent>               unitsStash;

        [Inject] private CueUIVisualizeConfig config;

        private SingletonFilter<CinemachineBrainComponent> cameraFilter;

        private IDisposable subscribeOnShoot;

        public override void OnAwake() {
            cameraFilter     = World.Filter.Singleton<CinemachineBrainComponent>();
            subscribeOnShoot = QuantumEvent.SubscribeManual<EventOnShoot>(OnShoot);
        }

        public override void Dispose() {
            subscribeOnShoot.Dispose();
        }

        public override void OnUpdate(float deltaTime) {
            var localRef = localPlayerSystem.LocalEntity;
            ref var cameraBrain = ref cameraFilter.Instance;

            for (var i = model.ShootsScreenNormalizedDirections.Count - 1; i >= 0; i--) {
                var data = model.ShootsScreenNormalizedDirections[i];

                if (mapperEntityRefToEntity.TryGet(data.SourceEntityRef, out var sourceEntity)) {
                    if (visibilityStash.Has(sourceEntity)) {
                        model.ShootsScreenNormalizedDirections.RemoveAt(i);
                        continue;
                    }

                    if (!localRef.IsNullOrDisposed() && unitsStash.Has(localRef) && unitsStash.Has(sourceEntity)) {
                        ref var localUnitComponent  = ref unitsStash.Get(localRef);
                        ref var sourceUnitComponent = ref unitsStash.Get(sourceEntity);

                        data.WorldPosition = sourceUnitComponent.PositionView;
                        data.ScreenPosition = cameraBrain.camera.WorldToScreenPoint(data.WorldPosition);
                        data.IsOnScreen = ScreenSpaceHelper.IsPositionOnScreen(data.ScreenPosition) && inFrustumStash.Has(sourceEntity);

                        var direction = sourceUnitComponent.PositionView - localUnitComponent.PositionView;

                        if (ScreenSpaceHelper.TryGetScreenDirection(direction, cameraBrain.Transform, out var newScreenDir)) {
                            data.ScreenNormalizedDirection = newScreenDir;
                        }
                    }
                }

                var needRemoveByTimer = data.Timer.ProcessTimer(deltaTime);
                if (needRemoveByTimer) {
                    model.ShootsScreenNormalizedDirections.RemoveAt(i);
                }
                else {
                    model.ShootsScreenNormalizedDirections[i] = data;
                }
            }
        }

        private unsafe void OnShoot(EventOnShoot callback) {
            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            var shooterRef = callback.unitRef;

            if (localRef == shooterRef) {
                return;
            }

            if (!updater.TryGetEntityView(localRef, out var localView)) {
                return;
            }

            if (!updater.TryGetEntityView(shooterRef, out var shooterViw)) {
                return;
            }

            if (!mapperEntityRefToEntity.TryGet(shooterRef, out var shooterEntity)) {
                return;
            }

            if (visibilityStash.Has(shooterEntity)) {
                return;
            }

            var shooterUnit = f.GetPointer<Unit>(shooterRef);
            if (!shooterUnit->TryGetActiveWeapon(f, out var shooterWeapon)) {
                return;
            }

            var localUnit = f.GetPointer<Unit>(localRef);

            var shotSoundRange = shooterWeapon->CurrentStats.weaponShotSoundRange.AsFloat;

            shotSoundRange *= 1 + localUnit->CurrentStats.audioDistance.AdditiveMultiplierMinus1.AsFloat;

            var shotSoundRangeSqr = shotSoundRange * shotSoundRange;

            var direction   = shooterViw.Transform.position - localView.Transform.position;
            var distanceSqr = direction.sqrMagnitude;

            if (distanceSqr <= shotSoundRangeSqr) {
                for (var i = 0; i < model.ShootsScreenNormalizedDirections.Count; i++) {
                    if (model.ShootsScreenNormalizedDirections[i].SourceEntityRef == shooterRef) {
                        return;
                    }
                }

                ref var cameraBrain = ref cameraFilter.Instance;

                if (!ScreenSpaceHelper.TryGetScreenDirection(direction, cameraBrain.Transform, out var screenDirection)) {
                    return;
                }

                var worldPos = shooterViw.Transform.position;
                var screenPos = cameraBrain.camera.WorldToScreenPoint(worldPos);
                var isOnScreen = ScreenSpaceHelper.IsPositionOnScreen(screenPos) && inFrustumStash.Has(shooterEntity);

                var listenedCue = new CueData {
                                ScreenNormalizedDirection = screenDirection,
                                Timer                     = config.shootMarkerLifetime,
                                SourceEntityRef           = shooterRef,
                                WorldPosition             = worldPos,
                                ScreenPosition            = screenPos,
                                IsOnScreen                = isOnScreen,
                };

                model.ShootsScreenNormalizedDirections.Add(listenedCue);
            }
        }
    }
}