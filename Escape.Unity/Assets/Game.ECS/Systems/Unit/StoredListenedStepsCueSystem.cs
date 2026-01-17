namespace Game.ECS.Systems.Unit {
    using Camera;
    using Components.Camera;
    using Components.Unit;
    using Domain.Game;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using Utilities;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public unsafe class StoredListenedStepsCueSystem : SystemBase {
        [Inject] private PhotonService                     photonService;
        [Inject] private ListenedCueModel                  model;
        [Inject] private LocalPlayerSystem                 localPlayerSystem;
        [Inject] private Stash<UnitComponent>              unitsStash;
        [Inject] private Stash<VisiblyInFrustumMarker>     visibilityStash;
        [Inject] private Stash<InLocalCameraFrustumMarker> inFrustumStash;
        [Inject] private CueUIVisualizeConfig              config;
        [Inject] private MapperUnitEntityRefToEntitySystem mapperEntityRefToEntity;

        private Filter notVisiblyEnemy;

        private SingletonFilter<CinemachineBrainComponent> cameraFilter;

        public override void Dispose() {
            model.ShootsScreenNormalizedDirections.Clear();
            model.StepsScreenNormalizedDirections.Clear();
        }

        public override void OnAwake() {
            cameraFilter = World.Filter.Singleton<CinemachineBrainComponent>();

            notVisiblyEnemy = World.Filter
                            .With<UnitComponent>()
                            .With<EnemyUnitMarker>()
                            .Without<VisiblyInFrustumMarker>()
                            .Build();
        }

        public override void OnUpdate(float deltaTime) {
            var localRef = localPlayerSystem.LocalEntity;
            ref var cameraBrain = ref cameraFilter.Instance;

            for (var i = model.StepsScreenNormalizedDirections.Count - 1; i >= 0; i--) {
                var data = model.StepsScreenNormalizedDirections[i];

                if (mapperEntityRefToEntity.TryGet(data.SourceEntityRef, out var sourceEntity)) {
                    if (visibilityStash.Has(sourceEntity)) {
                        model.StepsScreenNormalizedDirections.RemoveAt(i);
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
                    model.StepsScreenNormalizedDirections.RemoveAt(i);
                }
                else {
                    model.StepsScreenNormalizedDirections[i] = data;
                }
            }

            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localQuantumRef)) {
                return;
            }

            if (localRef.IsNullOrDisposed()) {
                return;
            }

            if (!unitsStash.Has(localRef)) {
                return;
            }

            ref var localUnit = ref unitsStash.Get(localRef);

            var sourceUnit = f.GetPointer<Unit>(localQuantumRef);

            var distance = config.stepListenDistance;

            distance *= 1 + sourceUnit->CurrentStats.audioDistance.AdditiveMultiplierMinus1.AsFloat;

            var distanceSqr = distance * distance;

            foreach (var entity in notVisiblyEnemy) {
                ref var unit = ref unitsStash.Get(entity);

                var direction = unit.PositionView - localUnit.PositionView;
                if (direction.sqrMagnitude > distanceSqr) {
                    continue;
                }

                if (!f.TryGetPointer<KCC>(unit.EntityRef, out var kcc)) {
                    continue;
                }

                if (config.realSpeedLimit > kcc->RealSpeed) {
                    continue;
                }

                var hasActiveShootIndicator = false;
                for (var i = 0; i < model.ShootsScreenNormalizedDirections.Count; i++) {
                    if (model.ShootsScreenNormalizedDirections[i].SourceEntityRef == unit.EntityRef) {
                        hasActiveShootIndicator = true;
                        break;
                    }
                }

                if (hasActiveShootIndicator) {
                    continue;
                }

                var hasActiveStepIndicator = false;
                for (var i = 0; i < model.StepsScreenNormalizedDirections.Count; i++) {
                    if (model.StepsScreenNormalizedDirections[i].SourceEntityRef == unit.EntityRef) {
                        hasActiveStepIndicator = true;
                        break;
                    }
                }

                if (hasActiveStepIndicator) {
                    continue;
                }

                if (!ScreenSpaceHelper.TryGetScreenDirection(direction, cameraBrain.Transform, out var screenDir)) {
                    continue;
                }

                var worldPos = unit.PositionView;
                var screenPos = cameraBrain.camera.WorldToScreenPoint(worldPos);
                var isOnScreen = ScreenSpaceHelper.IsPositionOnScreen(screenPos) && inFrustumStash.Has(entity);

                var listenedCue = new CueData {
                                ScreenNormalizedDirection = screenDir,
                                Timer                     = config.stepMarkerLifetime,
                                SourceEntityRef           = unit.EntityRef,
                                WorldPosition             = worldPos,
                                ScreenPosition            = screenPos,
                                IsOnScreen                = isOnScreen,
                };

                model.StepsScreenNormalizedDirections.Add(listenedCue);
            }
        }
    }
}