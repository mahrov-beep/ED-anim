namespace Game.ECS.Systems.Unit {
    using Camera;
    using Game.ECS.Components.Unit;
    using Core;
    using Player;
    using Game.Services.Photon;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class DebuffBurnVisualSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private QuantumEntityViewSystem entityViewUpdater;

        [Inject] private Stash<UnitComponent>          unitStash;
        [Inject] private Stash<EnemyUnitMarker>        enemiesStash;
        [Inject] private Stash<VisiblyInFrustumMarker> visiblyInFrustumStash;
        [Inject] private Stash<LocalCharacterMarker>   localCharacterStash;

        private Filter filter;

        public override void OnAwake() {
            filter = World.Filter
                            .With<UnitComponent>()
                            .Build();
        }

        public override unsafe void OnUpdate(float deltaTime) {
            if (photonService.PredictedFrame is not { } f) {
                return;
            }

            foreach (var entity in filter) {
                ref var unit = ref unitStash.Get(entity);

                if (!visiblyInFrustumStash.Has(entity) && !localCharacterStash.Has(entity)) {
                    continue;
                }

                if (ReferenceEquals(unit.debuffVisual, null)) {
                    return;
                }

                var debuffVisual = unit.debuffVisual.BurnDebuff;

                if (!f.TryGetPointer<Health>(unit.EntityRef, out var h)) {
                    continue;
                }

                var modifiers = f.ResolveList(h->Modifiers);

                if (modifiers.Count < 1) {
                    debuffVisual.Stop();
                    continue;
                }

                for (var i = 0; i < modifiers.Count; i++) {
                    var m = modifiers[i];
                    if (m.Appliance != HealthAttributeAppliance.Continuous) {
                        continue;
                    }

                    if (m.DamageType == EDamageType.Fire) {

                        debuffVisual.Play();

                        return;
                    }
                }

                debuffVisual.Stop();
            }
        }
    }
}