namespace Game.ECS.Systems.Unit {
    using Components.Unit;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class MarkLocalUnitSystem : SystemBase {
        [Inject] private PhotonService             photonService;
        [Inject] private Stash<UnitComponent>      unitComponent;
        [Inject] private Stash<LocalCharacterMarker> localUnitComponent;

        private Filter unitFilter;

        public override void OnAwake() {
            this.unitFilter = this.World.Filter
                .With<UnitComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            var game = this.photonService.Runner!.Game;

            foreach (var entity in this.unitFilter) {
                ref var unit = ref this.unitComponent.Get(entity);

                var quantumEntity = unit.EntityRef;

                if (!f.TryGet(quantumEntity, out Unit unitQ)) {
                    continue;
                }

                var isLocal = game.PlayerIsLocal(unitQ.PlayerRef);

                if (isLocal) {
                    this.localUnitComponent.Set(entity, new LocalCharacterMarker());
                }
                else {
                    this.localUnitComponent.Remove(entity);
                }
            }
        }
    }
}