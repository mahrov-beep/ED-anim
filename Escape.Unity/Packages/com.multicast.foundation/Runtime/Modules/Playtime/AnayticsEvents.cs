namespace Multicast.Modules.Playtime {
    using Multicast.Analytics;

    public class PlayTimeAnalyticsEvent : IAnalyticsEvent {
        public int PlayTimeMinutes { get; }

        public string Name => "playTime";

        public PlayTimeAnalyticsEvent(int playTimeMinutes) {
            this.PlayTimeMinutes = playTimeMinutes;
        }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("playTime", this.PlayTimeMinutes);
    }
}