namespace Game.Shared.UserProfile.Data.Store {
    using Multicast.ServerData;

    public class SdStoreItem : SdArrayObject { 
        public SdValue<bool> HasBeenSeen    { get; }
        public SdValue<int>  PurchasedCount { get; }

        public SdStoreItem(SdArgs args) : base(args) {
            this.HasBeenSeen    = this.Child(0);
            this.PurchasedCount = this.Child(1);
        }
    }
}