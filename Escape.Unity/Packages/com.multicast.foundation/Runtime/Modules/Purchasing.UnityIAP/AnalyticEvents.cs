namespace Multicast.Modules.Purchasing.UnityIAP {
    using Multicast.Analytics;

    public class UnityIapInitializationFailed : IAnalyticsEvent {
        public UnityIapInitializationFailed(string error, string message) {
            this.Error   = error;
            this.Message = message;
        }

        public string Name    => "unity_iap_initialization_failed";
        public string Error   { get; }
        public string Message { get; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("error", this.Error)
            .Add("message", this.Message);
    }

    public class UnityIapInitializationSucceed : IAnalyticsEvent {
        public string                 Name => "unity_iap_initialization_succeed";
        public AnalyticsArgCollection Args => new();
    }

    public class UnityIapPurchaseFailed : IAnalyticsEvent {
        public string Name => "unity_iap_purchase_failed";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("product", this.ProductId)
            .Add("error_msg", this.ErrorMessage)
            .Add("store_error_code", this.StoreErrorCode)
            .Add("store_error_msg", this.StoreErrorMsg);

        public string ProductId     { get; }
        public string StoreErrorMsg { get; }

        public string StoreErrorCode { get; }

        public string ErrorMessage { get; }


        public UnityIapPurchaseFailed(string productId, string storeErrorMsg, string storeErrorCode, string errorMessage) {
            this.StoreErrorMsg  = storeErrorMsg;
            this.StoreErrorCode = storeErrorCode;
            this.ErrorMessage   = errorMessage;
            this.ProductId      = productId;
        }
    }
}