namespace Game.Domain.GameInventory {
    using Multicast;
    using Quantum;
    using UniMob;

    public class GameNearbyInteractiveZoneModel : Model {
        public GameNearbyInteractiveZoneModel(Lifetime lifetime) : base(lifetime) {
        }

        [Atom] public EntityRef NearbyInteractiveZone { get; set; }
    }
}