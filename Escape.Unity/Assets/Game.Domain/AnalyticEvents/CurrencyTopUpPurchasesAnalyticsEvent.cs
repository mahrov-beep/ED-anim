namespace Game.Domain.AnalyticEvents {
    using Multicast.Analytics;

    public class CurrencyTopUpPurchasesAnalyticsEvent : IAnalyticsEvent {
        public string Name     => "CurrencyTopUpShow";
        public string Currency { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("Currency", this.Currency);
    }
}