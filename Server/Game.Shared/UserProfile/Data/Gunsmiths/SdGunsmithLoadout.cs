namespace Game.Shared.UserProfile.Data.Gunsmiths {
    using Multicast.ServerData;
    using Quantum;

    public class SdGunsmithLoadout : SdArrayObject {
        public string Guid => this.GetSdObjectKey();

        public SdValue<string>              GunsmithLoadoutKey { get; }
        public SdValue<GameSnapshotLoadout> Loadout            { get; }

        public SdGunsmithLoadout(SdArgs args) : base(args) {
            this.Loadout            = this.Child(0);
            this.GunsmithLoadoutKey = this.Child(1);
        }
    }
}