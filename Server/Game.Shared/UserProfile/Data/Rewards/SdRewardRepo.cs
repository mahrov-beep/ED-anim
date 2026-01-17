namespace Game.Shared.UserProfile.Data.Rewards {
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using Multicast.Numerics;
    using Multicast.ServerData;

    public class SdRewardRepo : SdObject {
        public SdDict<SdReward> Dict { get; }

        public SdRewardRepo(SdArgs args) : base(args) {
            this.Dict = new SdDict<SdReward>(this.Child(0), a => new SdReward(a));
        }

        [PublicAPI]
        public bool IsEmpty => this.Dict.Count == 0;

        [PublicAPI]
        public SdReward First => this.Dict.OrderBy(it => it.Index).First();

        [PublicAPI]
        public SdReward Get(string guid) {
            return this.Dict.Get(guid);
        }

        [PublicAPI]
        public bool Contains(string guid) {
            return this.Dict.ContainsKey(guid);
        }

        [PublicAPI]
        public SdReward Dequeue(string guid) {
            var udReward = this.Dict.Get(guid);
            this.Dict.Remove(udReward);
            return udReward;
        }

        [PublicAPI]
        internal SdReward CreateInternal(string guid, Reward reward) {
            var num = this.Dict.Count == 0
                ? 0
                : this.Dict.Max(it => it.Index) + 1;

            var udReward = this.Dict.Create(guid);
            udReward.Set(reward, num);
            return udReward;
        }
    }
}