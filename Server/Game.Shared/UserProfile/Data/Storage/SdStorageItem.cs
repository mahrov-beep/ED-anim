namespace Game.Shared.UserProfile.Data.Storage {
    using Multicast.ServerData;
    using Quantum;

    public class SdStorageItem : SdArrayObject {
        public string ItemGuid => this.GetSdObjectKey();

        public SdValue<GameSnapshotLoadoutItem> Item    { get; }
        public SdValue<int>                     IndexI  { get; }
        public SdValue<int>                     IndexJ  { get; }
        public SdValue<bool>                    Rotated { get; }

        public SdStorageItem(SdArgs args) : base(args) {
            this.Item    = this.Child(0);
            this.IndexI  = this.Child(1);
            this.IndexJ  = this.Child(2);
            this.Rotated = this.Child(3);
        }
    }
}