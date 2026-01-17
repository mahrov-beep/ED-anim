namespace Game.ECS.Systems.Unit {
    using _Project.Scripts.GameView;
    using Components.Unit;
    using Game.Domain.GameProperties;
    using Game.ECS.Scripts.GameView;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    using Multicast;
    using Multicast.GameProperties;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ReconOutlineSystem : SystemBase {
        [Inject] private PhotonService photonService;
        [Inject] private LocalPlayerSystem localPlayerSystem;
        [Inject] private Stash<UnitComponent> stashUnit;
        [Inject] private Stash<EnemyUnitMarker> stashEnemyMarker;
        [Inject] private GamePropertiesModel gameProperties;
        private Filter filterEnemyUnits;

        public override void OnAwake() {
            this.filterEnemyUnits = this.World.Filter
                .With<UnitComponent>()
                .With<EnemyUnitMarker>()
                .Build();
        }

        public override void OnUpdate(float deltaTime) {
            if (photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!f.TryGet<Team>(localRef, out var localTeam)) {
                return;
            }

            foreach (var enemyEntity in this.filterEnemyUnits) {
                ref var enemyUnit = ref this.stashUnit.Get(enemyEntity);

                if (!f.Exists(enemyUnit.EntityRef)) {
                    continue;
                }

                var alwaysShowOutline = this.gameProperties != null &&
                                       this.gameProperties.Get(ReconGameProperties.Booleans.AlwaysShowEnemyOutline);

                bool isDetected = alwaysShowOutline ||
                                  IsDetectedByLocalRecon(f, localTeam, enemyUnit.EntityRef);

                var entityView = enemyUnit.quantumEntityView;
                if (entityView == null) {
                    continue;
                }

                var outline = enemyUnit.reconOutline;
                if (outline != null) {
                    outline.SetOutline(isDetected);
                }
            }
        }

        private static bool IsDetectedByLocalRecon(Frame f, Team localTeam, EntityRef targetRef) {
            if (!EAttributeType.Set_ReconDetected.IsValueSet(f, targetRef)) {
                return false;
            }

            if (!f.TryGet<Team>(targetRef, out var targetTeam) && targetTeam == localTeam) {
                return false;
            }

            return true;
        }
    }
}
