namespace Game.Shared.UserProfile.Data.Features {
    using Multicast.ServerData;

    public class SdFeature : SdArrayObject {
        public SdValue<bool> Unlocked { get; }
        public SdValue<bool> Viewed   { get; }

        public SdFeature(SdArgs args) : base(args) {
            this.Unlocked = this.Child(0);
            this.Viewed   = this.Child(1);
        }
    }
}