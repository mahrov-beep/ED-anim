namespace Game.Domain {
    using Multicast;
    using UniMob;

    public class SystemModel : Model {
        public SystemModel(Lifetime lifetime) : base(lifetime) {
        }

        [Atom] public bool IsInternetConnectionLost { get; set; }
    }
}