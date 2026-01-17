namespace Multicast.Server {
    using System;
    using Multicast.Analytics;

    [Serializable, RequireFieldsInit]
    public class ServerRequestNetworkErrorAnalyticEvent : IAnalyticsEvent {
        public string errorCategory;
        public string requestUrl;
        public int    errorCode;
        public string errorMessage;

        public string Name => "server_network_error";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("category", this.errorCategory) {
                new AnalyticsArg("url", this.requestUrl) {
                    new AnalyticsArg("code", this.errorCode) {
                        new AnalyticsArg("message", this.errorMessage),
                    },
                }
            });
    }
}