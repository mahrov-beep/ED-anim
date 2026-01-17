namespace Multicast.Modules.UserData {
    using Multicast.Analytics;

    public class FailedToLoadUserDataAnalyticsEvent : IAnalyticsEvent {
        public FailedToLoadUserDataAnalyticsEvent(string source, string error) {
            this.Source = source;
            this.Error  = error;
        }

        public string Name => "user_data_load_failed";

        public string Source { get; }
        public string Error  { get; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("source", this.Source)
            .Add("error", this.Error);
    }

    public class FailedToSaveUserDataAnalyticsEvent : IAnalyticsEvent {
        public FailedToSaveUserDataAnalyticsEvent(string error) {
            this.Error = error;
        }

        public string Name => "user_data_save_failed";

        public string Error { get; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("error", this.Error);
    }
}