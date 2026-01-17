namespace Game.Domain.AnalyticEvents {
    using Multicast.Analytics;
    using Multicast.Numerics;

    public class SpentHardCurrencyAnalyticsEvent : IAnalyticsEvent {
        public string Name        => "SpentHardCurrency";
        public string PurchaseKey { get; }
        public string CategoryKey { get; }
        public string Currency    { get; }
        public long   Amount      { get; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("purchase_key", this.CategoryKey + this.PurchaseKey)
            .Add("category_key", this.CategoryKey)
            .Add("currency", this.Currency)
            .Add("amount", this.Amount);

        public SpentHardCurrencyAnalyticsEvent(string purchaseKey, string categoryKey, string currency, long amount) {
            this.PurchaseKey = purchaseKey;
            this.CategoryKey = categoryKey;
            this.Currency    = currency;
            this.Amount      = amount;
        }
    }
}