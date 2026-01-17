namespace Game.Shared.UserProfile.Data.Loadouts {
    using DTO;
    using Multicast.ServerData;
    using Quantum;

    public class SdLoadout : SdArrayObject {
        public string Guid => this.GetSdObjectKey();

        public SdValue<string> LockedForGame { get; }

        public SdValue<GameSnapshotLoadout> LoadoutSnapshot { get; }

        public SdLoadout(SdArgs args) : base(args) {
            this.LockedForGame   = this.Child(0);
            this.LoadoutSnapshot = this.Child(1);
        }
    }
}