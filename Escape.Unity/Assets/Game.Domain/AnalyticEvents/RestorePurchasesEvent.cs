namespace Game.Domain.AnalyticEvents {
    using Multicast.Analytics;

    public class RestorePurchasesSucceedEvent : IAnalyticsEvent {
        public int Count { get; set; }

        public string Name => "restore_purchases_succeed";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("restore_purchases", this.Count);
    }
    
    public class RestorePurchasesFailedEvent : IAnalyticsEvent {
        public string ErrorMessage { get; set; }

        public string Name => "restore_purchases_failed";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("error", this.ErrorMessage);
    }
    
    public class RestorePurchasesInitiatedAnalyticsEvent : IAnalyticsEvent {
        public string PurchaseKey { get; set; }

        public string Name => "restore_purchases_initiated";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection();
    }
}