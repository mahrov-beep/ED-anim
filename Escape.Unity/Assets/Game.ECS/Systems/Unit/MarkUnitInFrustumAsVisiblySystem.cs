namespace Game.ECS.Systems.Unit {
    using Camera;
    using Components.Camera;
    using Components.Unit;
    using Multicast;
    using Photon.Deterministic;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    /// <summary>
    /// для юнитов с InLocalCameraFrustumMarker
    /// проверяем LineOfSightHelper.AnyStaticBetween
    /// от позиции камеры до позиции юнита
    /// если препятсвий нет, добавляем юниту InLineSightMarker, иначе удаляем.
    /// </remarks>
    public class MarkUnitInFrustumAsVisiblySystem : SystemBase {
        [Inject] private PhotonService photonService;

        [Inject] private Stash<VisiblyInFrustumMarker> visiblyInFrustumStash;
        [Inject] private Stash<UnitComponent>          unitsStash;

        [Inject] private LocalPlayerSystem localPlayerSystem;
        [Inject] private CueUIVisualizeConfig config;

        private Filter inFrustumFilter;
        private Filter lostFrustumFilter;

        private SingletonFilter<CinemachineBrainComponent> cameraFilter;

        public override void Dispose() {
            visiblyInFrustumStash.RemoveAll();
        }

        public override void OnAwake() {
            cameraFilter = World.Filter.Singleton<CinemachineBrainComponent>();

            inFrustumFilter = World.Filter
                            .With<UnitComponent>()
                            .With<InLocalCameraFrustumMarker>()
                            .Without<LocalCharacterMarker>()
                            .Build();

            lostFrustumFilter = World.Filter
                            .With<VisiblyInFrustumMarker>()
                            .Without<InLocalCameraFrustumMarker>()
                            .Build();
        }

        public override void OnUpdate(float deltaTime) {
            if (photonService.PredictedFrame is not { } f) {
                return;
            }

            foreach (var entity in lostFrustumFilter) {
                visiblyInFrustumStash.Remove(entity);
            }

            var camera = cameraFilter.Instance.camera;
            var cameraPosition = camera.transform.position;  // Keep as Vector3 for Unity operations
            var cameraPositionFP = cameraPosition.ToFPVector3();  // FP version for Quantum raycast
            var enableDebug = config.debugVisibilityRaycasts;
            var debugDuration = config.debugRaycastDuration;  // Use float directly
            var maxSqrDistance = config.visibilityCheckDistance * config.visibilityCheckDistance;

            foreach (var entity in inFrustumFilter) {
                ref var unit = ref unitsStash.Get(entity);

                // Use actual chest position from AimAssistTarget
                var aimAssistTarget = unit.AimAssistTarget;
                var chestPosition   = aimAssistTarget.transform.position + Vector3.up;
                if (aimAssistTarget.TryGetComponent(out Collider aimAssistCollider)) {
                    chestPosition = aimAssistCollider.bounds.center;
                }

                // Check distance first (squared for performance)
                var sqrDistance = (chestPosition - cameraPosition).sqrMagnitude;

                if (sqrDistance > maxSqrDistance) {
                    visiblyInFrustumStash.Remove(entity);
                    continue;  // Skip distant enemies
                }

                // Single raycast to actual chest position
                var chestPositionFP = chestPosition.ToFPVector3();
                var blocked = LineOfSightHelper.AnyStaticBetween(f, cameraPositionFP, chestPositionFP);

                if (!blocked) {
                    visiblyInFrustumStash.Set(entity, new VisiblyInFrustumMarker());

                    // Debug visualization - green for visible
                    #if UNITY_EDITOR
                    if (enableDebug) {
                        UnityEngine.Debug.DrawLine(
                            cameraPosition,
                            chestPosition,
                            UnityEngine.Color.green,
                            debugDuration);
                    }
                    #endif
                }
                else {
                    visiblyInFrustumStash.Remove(entity);

                    // Debug visualization - red for blocked
                    #if UNITY_EDITOR
                    if (enableDebug) {
                        UnityEngine.Debug.DrawLine(
                            cameraPosition,
                            chestPosition,
                            UnityEngine.Color.red,
                            debugDuration);
                    }
                    #endif
                }
            }
        }
    }
}
