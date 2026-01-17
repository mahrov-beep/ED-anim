namespace Game.Shared.UserProfile.Data {
    using Multicast.Numerics;
    using Multicast.ServerData;
    using Quantum;

    public class SdGameResult : SdArrayObject {
        public string GameId => this.GetSdObjectKey();

        public SdValue<bool>        IsPlaying     { get; }
        public SdValue<GameTime>    StartTime     { get; }
        public SdValue<GameResults> GameResult    { get; }
        public SdValue<bool>        RewardClaimed { get; }

        public SdGameResult(SdArgs args) : base(args) {
            this.IsPlaying     = this.Child(0);
            this.StartTime     = this.Child(1);
            this.GameResult    = this.Child(2);
            this.RewardClaimed = this.Child(3);
        }
    }
}