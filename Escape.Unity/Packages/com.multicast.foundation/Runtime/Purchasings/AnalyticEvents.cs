namespace Multicast.Purchasing {
    using System.Globalization;
    using Analytics;

    public class PurchaseInitiatedAnalyticsEvent : IAnalyticsEvent {
        public string PurchaseKey { get; set; }

        public string Name => "purchase_initiated";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("purchase_key", this.PurchaseKey);
    }

    public class PurchaseEndAnalyticsEvent : IAnalyticsEvent {
        public string PurchaseKey { get; set; }

        public string Name => "purchase_end";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("purchase_key", this.PurchaseKey);
    }

    public class PurchaseFailedAnalyticsEvent : IAnalyticsEvent {
        public string PurchaseKey  { get; set; }
        public string ErrorMessage { get; set; }

        public string Name => "purchase_failed";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("purchase_key", this.PurchaseKey)
            .Add("error", this.ErrorMessage);
    }

    public class PurchaseCancelledAnalyticsEvent : IAnalyticsEvent {
        public string PurchaseKey { get; set; }

        public string Name => "purchase_cancelled";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("purchase_key", this.PurchaseKey);
    }

    public class PurchaseAnalyticsEvent : IAnalyticsEvent {
        public string  PurchaseKey     { get; set; }
        public string  StoreProductId  { get; set; }
        public string  IsoCurrencyCode { get; set; }
        public decimal LocalizedPrice  { get; set; }
        public int     PriceUsdCents   { get; set; }
        public string  TransactionID   { get; set; }

        public string Name => "purchase";

        public virtual AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("purchase_key", this.PurchaseKey)
            .Add("store_product", this.StoreProductId)
            .Add("iso_currency_code", this.IsoCurrencyCode)
            .Add("localized_price", this.LocalizedPrice.ToString(CultureInfo.InvariantCulture))
            .Add("price_usd_cents", this.PriceUsdCents)
            .Add("transaction", this.TransactionID);
    }

    public class AppStorePurchaseAnalyticsEvent : PurchaseAnalyticsEvent {
        public string Payload { get; set; }
    }

    public class GooglePlayPurchaseAnalyticsEvent : PurchaseAnalyticsEvent {
        public string GoogleSignature { get; set; }
        public string GoogleJsonData  { get; set; }
    }
}