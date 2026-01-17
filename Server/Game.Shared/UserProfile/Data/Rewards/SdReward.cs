namespace Game.Shared.UserProfile.Data.Rewards {
    using JetBrains.Annotations;
    using Multicast.Numerics;
    using Multicast.ServerData;

    public class SdReward : SdArrayObject {
        private readonly SdValue<Reward> udReward;
        private readonly SdValue<int>    udIndex;

        [PublicAPI] public Reward Reward => this.udReward.Value;
        [PublicAPI] public string Guid   => this.GetSdObjectKey();

        internal int Index => this.udIndex.Value;

        public SdReward(SdArgs args) : base(args) {
            this.udReward = this.Child(0);
            this.udIndex  = this.Child(1);
        }

        internal void Set(Reward reward, int index) {
            this.udReward.Value = reward;
            this.udIndex.Value  = index;
        }
    }
}