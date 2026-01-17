namespace Game.Shared.UserProfile.Data.CoinFarms {
    using Multicast.Numerics;
    using Multicast.ServerData;

    public class SdCoinFarm : SdArrayObject {
        public SdValue<GameTime> LastCollectTime { get; }

        public SdCoinFarm(SdArgs args) : base(args) {
            this.LastCollectTime = this.Child(0);
        }
    }
}