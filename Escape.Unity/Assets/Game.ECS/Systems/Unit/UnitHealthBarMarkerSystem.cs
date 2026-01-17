namespace Game.ECS.Systems.Unit {
    using Components.Unit;
    using Core;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class UnitHealthBarMarkerSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;

        [Inject] private Stash<UnitComponent>       unitComponent;
        [Inject] private Stash<UnitHealthBarMarker> healthBarMarker;

        private Filter unitFilter;

        public override void OnAwake() {
            this.unitFilter = this.World.Filter
                .With<UnitComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime) {
            foreach (var entity in this.unitFilter) {
                if (this.IsHealthBarVisible(entity)) {
                    this.healthBarMarker.Set(entity, new UnitHealthBarMarker {
                        alpha = 1f,
                    });
                }
                else if (this.healthBarMarker.Has(entity)) {
                    ref var health = ref this.healthBarMarker.Get(entity);

                    health.alpha -= deltaTime * 5f;

                    if (health.alpha <= 0) {
                        this.healthBarMarker.Remove(entity);
                    }
                }
            }
        }

        private bool IsHealthBarVisible(Entity otherUnitEntity) {
            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (!f.TryGet(localRef, out Unit localUnit)) {
                return false;
            }

            var ohterUnitEntityRef = this.unitComponent.Get(otherUnitEntity).EntityRef;

            return localUnit.HasTarget && localUnit.Target == ohterUnitEntityRef;
        }
    }
}