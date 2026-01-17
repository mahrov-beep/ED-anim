namespace Game.ECS.Systems.GameInventory {
    using Domain.GameInventory;
    using Multicast;
    using Player;
    using Quantum;
    using Services.Photon;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class GameNearbyInteractiveZoneSystem : SystemBase {
        [Inject] private PhotonService                  photonService;
        [Inject] private LocalPlayerSystem              localPlayerSystem;
        [Inject] private GameNearbyInteractiveZoneModel interactiveZoneModel;

        public override void OnAwake() {
        }

        public override void Dispose() {
            base.Dispose();

            this.interactiveZoneModel.NearbyInteractiveZone = EntityRef.None;
        }

        public override void OnUpdate(float deltaTime) {
            this.interactiveZoneModel.NearbyInteractiveZone = this.TryGetNearbyInteractiveZone(out var zone)
                ? zone
                : EntityRef.None;
        }

        private unsafe bool TryGetNearbyInteractiveZone(out EntityRef zone) {
            zone = EntityRef.None;

            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (f.Global->GameState != EGameStates.Game) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out Unit unit)) {
                return false;
            }

            if (!f.TryGet(unit.NearbyInteractiveZone, out InteractiveZone interactiveZone)) {
                return false;
            }

            var zoneAsset = f.FindAsset(interactiveZone.Asset);

            if (!zoneAsset.CanInteract(f, unit.NearbyInteractiveZone, localRef)) {
                return false;
            }

            if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, localRef) ||
                f.Has<UnitExited>(localRef)) {
                return false;
            }

            zone = unit.NearbyInteractiveZone;
            return true;
        }
    }
}
