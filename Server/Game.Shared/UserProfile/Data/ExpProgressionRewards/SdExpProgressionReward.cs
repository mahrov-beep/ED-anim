namespace Game.Shared.UserProfile.Data.ExpProgressionRewards {
    using Multicast.ServerData;

    public class SdExpProgressionReward : SdArrayObject {
        public SdValue<bool> Claimed { get; }

        public SdExpProgressionReward(SdArgs args) : base(args) {
            this.Claimed = this.Child(0);
        }
    }
}