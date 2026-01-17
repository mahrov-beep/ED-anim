#if UNITY_PURCHASING
namespace Multicast.Modules.Purchasing.UnityIAP {
    using Multicast.Purchasing;
    using UnityEngine;
    using UnityEngine.Purchasing;

    [System.Serializable]
    public struct Receipt {
        public string Store;
        public string TransactionID;
        public string Payload;
    }

    [System.Serializable]
    public struct PayloadAndroid {
        public string json;
        public string signature;
    }

    public class GooglePlaySucceedPurchaseDetails : PurchaseResult.SucceedPurchasePlatformDetails {
        private readonly PurchaseDef def;
        private readonly Product     product;

        public GooglePlaySucceedPurchaseDetails(PurchaseDef def, Product product) {
            this.def     = def;
            this.product = product;
        }

        public override PurchaseAnalyticsEvent BuildPurchaseEvent() {
            Receipt        receipt        = default;
            PayloadAndroid payloadAndroid = default;

            if (this.product.receipt != null) {
                receipt        = JsonUtility.FromJson<Receipt>(this.product.receipt);
                payloadAndroid = JsonUtility.FromJson<PayloadAndroid>(receipt.Payload);
            }
            else {
                Debug.LogError("Failed to fetch receipt for product " + this.product.definition.id);
            }

            return new GooglePlayPurchaseAnalyticsEvent {
                PurchaseKey     = this.def.key,
                StoreProductId  = this.product.definition.storeSpecificId,
                IsoCurrencyCode = this.product.metadata.isoCurrencyCode,
                LocalizedPrice  = this.product.metadata.localizedPrice,
                PriceUsdCents   = this.def.priceUdsCents,

                TransactionID   = receipt.TransactionID,
                GoogleSignature = payloadAndroid.signature,
                GoogleJsonData  = payloadAndroid.json,
            };
        }
    }

    public class AppStoreSucceedPurchaseDetails : PurchaseResult.SucceedPurchasePlatformDetails {
        private readonly PurchaseDef def;
        private readonly Product     product;

        public AppStoreSucceedPurchaseDetails(PurchaseDef def, Product product) {
            this.def     = def;
            this.product = product;
        }

        public override PurchaseAnalyticsEvent BuildPurchaseEvent() {
            Receipt receipt = default;

            if (this.product.receipt != null) {
                receipt = JsonUtility.FromJson<Receipt>(this.product.receipt);
            }
            else {
                Debug.LogError("Failed to fetch receipt for product " + this.product.definition.id);
            }

            return new AppStorePurchaseAnalyticsEvent() {
                PurchaseKey     = this.def.key,
                StoreProductId  = this.product.definition.storeSpecificId,
                IsoCurrencyCode = this.product.metadata.isoCurrencyCode,
                LocalizedPrice  = this.product.metadata.localizedPrice,
                PriceUsdCents   = this.def.priceUdsCents,

                TransactionID = receipt.TransactionID,
                Payload       = receipt.Payload,
            };
        }
    }
}
#endif