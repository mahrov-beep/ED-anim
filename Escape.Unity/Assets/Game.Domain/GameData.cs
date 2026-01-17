namespace Game.Domain.UserData {
    using Multicast.DropSystem;
    using Multicast.FeatureToggles;
    using Multicast.GameProperties;
    using Multicast.Purchasing;
    using Multicast.UserData;
    using Multicast.UserStats;

    public class GameData : UdObject {
        // Core
        public UdGamePropertiesData Properties { get; }
        public UdUserStatsRepo      UserStats  { get; }
        public UdPurchasesRepo      Purchases  { get; }
        public UdDropRepo           Drops      { get; }

        // Game

        public GameData(UdArgs args) : base(args) {
            // Core
            this.Properties = new UdGamePropertiesData(this.Child("properties"));
            this.UserStats  = new UdUserStatsRepo(this.Child("user_stats"));
            this.Purchases  = new UdPurchasesRepo(this.Child("purchases"));
            this.Drops      = new UdDropRepo(this.Child("drops"));

            // Game
        }
    }
}