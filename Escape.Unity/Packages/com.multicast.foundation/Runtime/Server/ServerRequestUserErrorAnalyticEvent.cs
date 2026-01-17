namespace Multicast.Server {
    using System;
    using Analytics;

    [Serializable, RequireFieldsInit]
    public class ServerRequestUserErrorAnalyticEvent : IAnalyticsEvent {
        public string requestUrl;
        public string errorMessage;

        public string Name => "server_user_error";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("url", this.requestUrl) {
                new AnalyticsArg("message", this.errorMessage),
            });
    }
}