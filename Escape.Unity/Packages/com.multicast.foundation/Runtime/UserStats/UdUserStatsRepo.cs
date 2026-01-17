namespace Multicast.UserStats {
    using Numerics;
    using UserData;

    public class UdUserStatsRepo : UdObject {
        public UdValue<ProtectedInt> SessionsCount { get; }

        public UdValue<string> InstallGameVersion { get; }

        public   UdValue<ProtectedInt> PlayedDaysCount               { get; }
        internal UdValue<GameTime>     LastPlayedDaysCountUpdateDate { get; }

        public UdValue<GameTime> FirstOpenTime { get; }

        public UdValue<int>    AdImpressionCount { get; }
        public UdValue<double> AdRevenue         { get; }

        public UdValue<int>        PlaytimeMinutes { get; }
        public UdLocalValue<int>   PlaytimeSeconds { get; }
        public UdLocalValue<float> PlaytimeElapsed { get; }

        public UdUserStatsRepo(UdArgs args) : base(args) {
            this.SessionsCount                 = this.Child("sessions_count");
            this.InstallGameVersion            = this.Child("install_game_version");
            this.PlayedDaysCount               = this.Child("days_count");
            this.LastPlayedDaysCountUpdateDate = this.Child("last_update");
            this.FirstOpenTime                 = this.Child("first_open_time");
            this.AdImpressionCount             = this.Child("impression_count");
            this.AdRevenue                     = this.Child("revenue");
            this.PlaytimeMinutes               = this.Child("pt_minutes");
            this.PlaytimeSeconds               = this.Child("pt_seconds");
            this.PlaytimeElapsed               = this.Child("pt_elapsed");
        }

        public bool IsFirstSession() {
            return this.SessionsCount.Value == 1;
        }
    }
}