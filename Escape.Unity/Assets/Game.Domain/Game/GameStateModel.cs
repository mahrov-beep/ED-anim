namespace Game.Domain.Game {
    using Multicast;
    using Quantum;
    using UniMob;

    public class GameStateModel : Model {
        public GameStateModel(Lifetime lifetime) : base(lifetime) {
        }

        [Atom] public EGameStates GameState         { get; set; }
        [Atom] public int         SecondsToStateEnd { get; set; }
    }
}