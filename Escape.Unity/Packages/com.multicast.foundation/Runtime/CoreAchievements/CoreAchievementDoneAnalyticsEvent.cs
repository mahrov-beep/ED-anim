namespace Multicast.CoreAchievements {
    using Analytics;

    public class CoreAchievementDoneAnalyticsEvent : IAnalyticsEvent {
        public string Name => "core_achievement_done";

        public string Achievement { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("achievement", this.Achievement);
    }
}