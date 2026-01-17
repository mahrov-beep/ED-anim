namespace Multicast.Purchasing {
    using UnityEngine;

    public class SucceedPurchaseDetails : PurchaseResult.SucceedPurchasePlatformDetails {
        private readonly PurchaseDef def;

        public SucceedPurchaseDetails(PurchaseDef def) {
            this.def = def;
        }

        public override PurchaseAnalyticsEvent BuildPurchaseEvent() => new PurchaseAnalyticsEvent {
            PurchaseKey     = this.def.key,
            StoreProductId  = this.GetStoreProductId(),
            IsoCurrencyCode = "USD",
            LocalizedPrice  = this.def.priceUdsCents / 100.0m,
            PriceUsdCents   = this.def.priceUdsCents,
        };

        private string GetStoreProductId() {
            return Application.platform switch {
                RuntimePlatform.Android => this.def.androidID,
                RuntimePlatform.IPhonePlayer => this.def.iosID,
                _ => this.def.key,
            };
        }
    }
}