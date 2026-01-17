#if APPMETRICA_SDK

namespace Multicast.Modules.AppMetrica {
    using Multicast.Analytics;
    using Multicast.Purchasing;

    internal sealed class AppMetricaAnalyticsAdapter : IAnalyticsAdapter {
        public string Name { get; } = "AppMetrica";

        private AppMetricaSdkConfiguration configuration;

        public AppMetricaAnalyticsAdapter(AppMetricaSdkConfiguration configuration) {
            this.configuration = configuration;
        }
        
        public void Send(BakedAnalyticsEvent evt) {
            if (evt.Args.IsEmpty) {
                Io.AppMetrica.AppMetrica.ReportEvent(evt.Name);
            }
            else {
                Io.AppMetrica.AppMetrica.ReportEvent(evt.Name, Json.Serialize(evt.ToAppMetricaDictionary()));
            }

            if (this.configuration.NeedToSendPurchases && evt.SourceEvent is PurchaseAnalyticsEvent purchaseEvent) {
                this.SendPurchase(purchaseEvent);
            }
        }

        public void Flush() {
            Io.AppMetrica.AppMetrica.SendEventsBuffer();
        }

        private void SendPurchase(PurchaseAnalyticsEvent evt) {
            var revenue = new Io.AppMetrica.Revenue((long)(evt.LocalizedPrice * 1_000_000), evt.IsoCurrencyCode) {
                ProductID = evt.StoreProductId
            };
            var receipt = new Io.AppMetrica.Revenue.Receipt();

            switch (evt) {
                case GooglePlayPurchaseAnalyticsEvent googlePlayEvt:
                    receipt.Signature = googlePlayEvt.GoogleSignature;
                    receipt.Data      = googlePlayEvt.GoogleJsonData;
                    break;

                case AppStorePurchaseAnalyticsEvent appStoreEvt:
                    receipt.TransactionID = appStoreEvt.TransactionID;
                    receipt.Data          = appStoreEvt.Payload;
                    break;
            }

            revenue.ReceiptValue = receipt;
            Io.AppMetrica.AppMetrica.ReportRevenue(revenue);
            Io.AppMetrica.AppMetrica.SendEventsBuffer();
        }
    }
}

#endif