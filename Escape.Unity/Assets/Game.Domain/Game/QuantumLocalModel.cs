namespace Game.Domain.Game {
    using Multicast;
    using Photon.Deterministic;
    using Quantum;
    using UniMob;
    public class QuantumLocalModel : Model {
        public QuantumLocalModel(Lifetime lifetime) : base(lifetime) { }

        public Unit        Unit      { get; set; }
        public Team        Team      { get; set; }
        public Transform3D Transform { get; set; }
        public KCC         KCC       { get; set; }

        [Atom] public FPVector3 Position { get; set; }

    }
}