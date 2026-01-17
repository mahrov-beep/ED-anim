namespace Game.ECS.Systems.Unit {

    using Game.ECS.Components.Unit;
    using Game.Services.Photon;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class MarkEnemyUnitSystem : SystemBase {
        [Inject] private PhotonService photonService;

        [Inject] private LocalPlayerSystem localPlayerSystem;

        [Inject] private Stash<UnitComponent>        unitStash;
        [Inject] private Stash<LocalCharacterMarker> localUnitStash;
        [Inject] private Stash<EnemyUnitMarker>      enemyMarkerStash;

        private Filter unitFilter;

        public override void OnAwake() {
            unitFilter = World.Filter
                            .With<UnitComponent>()
                            .Build();
        }

        public override void OnUpdate(float deltaTime) {
            if (photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            var localPlayerTeam = f.Get<Team>(localRef);

            foreach (var entity in unitFilter) {
                if (localUnitStash.Has(entity)) {
                    continue;
                }

                ref var unit = ref unitStash.Get(entity);

                if (!unit.quantumEntityView) {
                    TryRemoveEnemyMarker(entity);
                }

                if (!f.TryGet(unit.EntityRef, out Team team)) {
                    continue;
                }

                if (team == localPlayerTeam) {
                    TryRemoveEnemyMarker(entity);

                    return;
                }
                
                if (!enemyMarkerStash.Has(entity)) {
                    if (Application.isEditor) {
                        if (!unit.quantumEntityView.name.Contains("enemy")) {
                            unit.quantumEntityView.name += $" (enemy {team.ToString()})";
                        }
                    }

                    enemyMarkerStash.Add(entity);
                }
            }

            return;

            void TryRemoveEnemyMarker(Entity entity) {
                if (enemyMarkerStash.Has(entity)) {
                    enemyMarkerStash.Remove(entity);
                }
            }
        }
    }
}