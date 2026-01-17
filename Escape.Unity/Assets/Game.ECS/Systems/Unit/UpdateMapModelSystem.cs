namespace Game.ECS.Systems.Unit {
    using System;
    using Camera;
    using Components.Unit;
    using Core;
    using Domain.Game;
    using Multicast;
    using Photon.Deterministic;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;
    public unsafe class UpdateMapModelSystem : SystemBase {
        [Inject] private PhotonService photonService;

        [Inject] private LocalPlayerSystem                 localPlayerSystem;
        [Inject] private QuantumEntityViewSystem           updater;
        [Inject] private MapperUnitEntityRefToEntitySystem mapperEntityRefToEntity;

        [Inject] private Stash<UnitComponent>          stashUnit;
        [Inject] private Stash<UnitOnMap>              stashUnitOnMap;
        [Inject] private Stash<VisiblyInFrustumMarker> visibilityStash;
        [Inject] private Stash<EnemyUnitMarker>        enemyStash;
        [Inject] private Stash<GrenadeComponent>       grenadeStash;
        [Inject] private Stash<GrenadeMarker>          grenadeMarkerStash;

        [Inject] private MapModel mapModel;

        [Inject] private MinimapConfig minimapConfig;

        private Filter filterEnemyUnits;
        private Filter filterGrenades;
        private Filter filterAllyUnits;

        private IDisposable onShot;

        public override void Dispose() {
            onShot.Dispose();

            mapModel.Enemies.Clear();
            mapModel.PartyMembers.Clear();
            mapModel.InterestPoints.Clear();
            mapModel.Waypoints.Clear();
            mapModel.ExitPoints.Clear();
            mapModel.SpawnedItemBoxes.Clear();
            mapModel.DroppedItemBoxes.Clear();
            mapModel.Grenades.Clear();

            stashUnitOnMap.RemoveAll();
        }

        public override void OnAwake() {
            filterEnemyUnits = World.Filter
                .With<UnitComponent>()
                .With<EnemyUnitMarker>()
                .Build();

            filterAllyUnits = World.Filter
                .With<UnitComponent>()
                .Without<LocalCharacterMarker>()
                .Without<EnemyUnitMarker>().Build();

            filterGrenades = World.Filter
                .With<GrenadeComponent>()
                .With<GrenadeMarker>()
                .Build();

            onShot = QuantumEvent.SubscribeManual<EventOnShoot>(OnShot);

            mapModel.VisiblyRadius = minimapConfig.baseVisiblyRadius;
        }

        private void OnShot(EventOnShoot callback) {
            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            var unit = f.GetPointer<Unit>(callback.unitRef);

            if (f.Context.IsLocalPlayer(unit->PlayerRef)) {
                return;
            }

            if (!mapperEntityRefToEntity.TryGet(callback.unitRef, out var entity)) {
                return;
            }

            if (!stashUnitOnMap.Has(entity)) {
                stashUnitOnMap.Set(entity);
            }

            ref var hideTimer = ref stashUnitOnMap.Get(entity).hideTimer;

            var weapon          = f.GetPointer<Weapon>(callback.weaponRef);
            var weaponItemAsset = weapon->GetConfig(f);

            var hideDuration = minimapConfig.baseShotVisiblyDuration + weaponItemAsset.minimapShotVisiblyDuration;

            hideTimer = FPMath.Max(hideTimer, hideDuration);
        }

        public override void OnUpdate(float deltaTime) {
            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            if (f.Global->GameState != EGameStates.Game) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            UpdateVisiblyRadius(f, localRef, deltaTime);
            ExitUpdate(f, localRef);
            WayUpdate(f, localRef);
            EnemyUpdate(f, deltaTime);
            GrenadeUpdate(f);
            PartyMembersUpdate(f);
        }

        private void UpdateVisiblyRadius(Frame f, EntityRef localRef, float dt) {
            var baseVisiblyRadius = minimapConfig.baseVisiblyRadius;

            EAttributeType.PercentBoost_MinimapRadius
                .UNSAFE_ApplyPercentMultiplierOn(ref baseVisiblyRadius, f, localRef);

            mapModel.VisiblyRadius = Mathf.Lerp(
                mapModel.VisiblyRadius,
                baseVisiblyRadius,
                dt * minimapConfig.visibleRadiusChangeSpeed);
        }

        private void ExitUpdate(Frame f, EntityRef localRef) {
            if (mapModel.ExitPoints.Count == 1) {
                return;
            }

            var localUnit = f.GetPointer<Unit>(localRef);
            var exitZone  = f.GetPointer<Transform3D>(localUnit->TargetExitZone);

            mapModel.ExitPoints.Clear();
            mapModel.ExitPoints.Add(exitZone->Position.ToUnityVector3());
        }

        private void WayUpdate(Frame f, EntityRef localRef) {
            var pathfinder = f.GetPointer<NavMeshPathfinder>(localRef);

            var waypointsCount = pathfinder->WaypointCount;

            if (waypointsCount == 0) {
                // костыли.
                // Связано с тем, что NavMeshPathfinder пересчитывает вейпоинты не в основном потоке, а
                // перед этим он сбрасывает текущие вейпоинты и несколько кадров число вейпоинтов = 0.
                // Поэтому мы просто рисуем старые вейпоинты пока идет пересчет.
                return;
            }

            mapModel.Waypoints.Clear();

            for (var i = 0; i < waypointsCount; i++) {
                mapModel.Waypoints.Add(pathfinder->GetWaypoint(f, i).ToUnityVector3());
            }
        }

        private void EnemyUpdate(Frame f, float deltaTime) {
            mapModel.Enemies.Clear();

            var fpDeltaTime = FP.FromFloat_UNSAFE(deltaTime);

            foreach (var entity in filterEnemyUnits) {
                if (!stashUnitOnMap.Has(entity)) {
                    stashUnitOnMap.Set(entity);
                }

                ref var unitOnMap = ref stashUnitOnMap.Get(entity);
                ref var unit      = ref stashUnit.Get(entity);

                var isDead = CharacterFsm.CurrentStateIs<CharacterStateDead>(f, unit.EntityRef);
                if (isDead) {
                    continue;
                }

                var isVisibly = visibilityStash.Has(entity);
                if (isVisibly) {
                    unitOnMap.hideTimer = minimapConfig.baseHideDuration;
                    mapModel.Enemies.Add(new UnitOnMapData {
                        WorldPosition = unit.PositionView,
                        alpha         = 1f,
                    });

                    continue;
                }

                var isForceVisibly = EAttributeType.Set_ForceVisibleOnMap.IsValueSet(f, unit.EntityRef);
                if (isForceVisibly) {
                    unitOnMap.hideTimer = minimapConfig.baseHideDuration;
                    mapModel.Enemies.Add(new UnitOnMapData {
                        WorldPosition = unit.PositionView,
                        alpha         = 1f,
                    });

                    continue;
                }

                var isHide = unitOnMap.hideTimer.ProcessTimer(fpDeltaTime);
                if (isHide) {
                    continue;
                }

                mapModel.Enemies.Add(new UnitOnMapData {
                    WorldPosition = unit.PositionView,
                    alpha         = (unitOnMap.hideTimer / minimapConfig.baseHideDuration).AsFloat,
                });
            }
        }

        private void PartyMembersUpdate(Frame f) {
            mapModel.PartyMembers.Clear();

            foreach (var entity in filterAllyUnits) {
                ref var unit = ref stashUnit.Get(entity);

                var isDead = CharacterFsm.CurrentStateIs<CharacterStateDead>(f, unit.EntityRef);
                if (isDead) {
                    continue;
                }

                mapModel.PartyMembers.Add(new UnitOnMapData {
                    WorldPosition = unit.PositionView,
                    alpha         = 1f,
                });
            }
        }

        private void GrenadeUpdate(Frame f) {
            mapModel.Grenades.Clear();

            foreach (var entity in filterGrenades) {
                ref var grenade = ref grenadeStash.Get(entity);
                mapModel.Grenades.Add(grenade.PositionView);
            }
        }
    }
}