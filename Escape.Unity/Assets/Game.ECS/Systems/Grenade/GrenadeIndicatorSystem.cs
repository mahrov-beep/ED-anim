namespace Game.ECS.Systems.Grenade {
    using System.Collections.Generic;
    using Camera;
    using Components.Camera;
    using Components.Unit;
    using Core;
    using Domain.Game;
    using Multicast;
    using Player;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using Utilities;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class GrenadeIndicatorSystem : SystemBase {
        [Inject] private readonly PhotonService photonService;
        [Inject] private readonly GrenadeIndicatorModel grenadeIndicatorModel;
        [Inject] private readonly GrenadeIndicatorConfig config;
        [Inject] private readonly LocalPlayerSystem localPlayerSystem;
        [Inject] private readonly Stash<UnitComponent> unitsStash;
        [Inject] private readonly Stash<GrenadeComponent> grenadeStash;

        private SingletonFilter<CinemachineBrainComponent> cameraFilter;
        private Filter grenadeFilter;

        private readonly List<GrenadeIndicatorData> tempIndicators = new(16);
        private float updateTimer;

        public override void OnAwake() {
            cameraFilter = World.FilterSingleton<CinemachineBrainComponent>();
            grenadeFilter = World.Filter
                .With<GrenadeComponent>()
                .With<GrenadeMarker>()
                .Build();
        }

        public override void Dispose() {
            grenadeIndicatorModel.GrenadeIndicators.Clear();
            tempIndicators.Clear();
        }

        public override void OnUpdate(float deltaTime) {
            if (!ShouldUpdate(deltaTime)) {
                return;
            }

            tempIndicators.Clear();

            if (!TryGetRequiredData(out var playerPos, out var camera)) {
                return;
            }

            CollectVisibleGrenadeIndicators(playerPos, camera);
            UpdateModelWithIndicators();
        }

        #region Update Logic

        private bool ShouldUpdate(float deltaTime) {
            if (config.updateInterval <= 0f) {
                return true;
            }

            updateTimer += deltaTime;
            if (updateTimer < config.updateInterval) {
                return false;
            }

            updateTimer = 0f;
            return true;
        }

        private bool TryGetRequiredData(out Vector3 playerPos, out Camera camera) {
            playerPos = Vector3.zero;
            camera = null;

            if (!photonService.TryGetPredicted(out _)) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out _)) {
                return false;
            }

            var localRef = localPlayerSystem.LocalEntity;
            if (localRef.IsNullOrDisposed() || !unitsStash.Has(localRef)) {
                return false;
            }

            ref var localUnit = ref unitsStash.Get(localRef);
            playerPos = localUnit.PositionView;

            if (!cameraFilter.Instance.brain || !cameraFilter.Instance.brain.OutputCamera) {
                return false;
            }

            camera = cameraFilter.Instance.brain.OutputCamera;
            return true;
        }

        private void CollectVisibleGrenadeIndicators(Vector3 playerPos, Camera camera) {
            foreach (var grenadeEntity in grenadeFilter) {
                if (tempIndicators.Count >= config.maxVisibleIndicators) {
                    break;
                }

                ref var grenade = ref grenadeStash.Get(grenadeEntity);

                if (!IsGrenadeInRange(playerPos, grenade)) {
                    continue;
                }

                var screenPos = camera.WorldToScreenPoint(grenade.PositionView);
                var isOnScreen = IsPositionOnScreen(screenPos);

                if (!isOnScreen && !config.showOffScreenIndicators) {
                    continue;
                }

                AddIndicator(grenade, screenPos, isOnScreen);
            }
        }

        private void UpdateModelWithIndicators() {
            grenadeIndicatorModel.GrenadeIndicators.Clear();
            grenadeIndicatorModel.GrenadeIndicators.AddRange(tempIndicators);
        }

        #endregion

        #region Helper Methods

        private bool IsGrenadeInRange(Vector3 playerPos, GrenadeComponent grenade) {
            var dangerRadius = grenade.ExplosionRadius * config.dangerRadiusMultiplier;
            var maxRange = Mathf.Min(dangerRadius, config.maxDetectionRange);

            return ScreenSpaceHelper.IsWithinDistance(playerPos, grenade.PositionView, maxRange);
        }

        private static bool IsPositionOnScreen(Vector3 screenPos) {
            return ScreenSpaceHelper.IsPositionOnScreen(screenPos);
        }

        private void AddIndicator(GrenadeComponent grenade, Vector3 screenPos, bool isOnScreen) {
            tempIndicators.Add(new GrenadeIndicatorData {
                WorldPosition = grenade.PositionView,
                ScreenPosition = screenPos,
                ExplosionRadius = grenade.ExplosionRadius,
                IsOnScreen = isOnScreen
            });
        }

        #endregion
    }
}

