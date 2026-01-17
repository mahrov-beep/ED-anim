namespace Game.ECS.Systems.Unit {
    using Camera;
    using Components.Unit;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class DebuffSlowVisualSystem : SystemBase {
        [Inject] private PhotonService photonService;

        [Inject] private LocalPlayerSystem localPlayerSystem;

        [Inject] private Stash<UnitComponent>          unitStash;
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
                    continue;
                }

                if (!f.TryGetPointer<SlowDebuff>(unit.EntityRef, out var debuff)) {
                    continue;
                }

                var debuffView = unit.debuffVisual.SlowDebuff;

                if (debuff->hitCount < 1) {
                    debuffView.Stop();
                    continue;
                }

                debuffView.Play();
            }
        }
    }
}