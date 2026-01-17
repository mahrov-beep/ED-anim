namespace Multicast.Purchasing {
    using JetBrains.Annotations;

    public abstract class PurchaseResult {
        [PublicAPI]
        public static PurchaseResult Succeed(PurchaseDef purchaseDef, SucceedPurchasePlatformDetails details) {
            return new PurchaseSucceed(purchaseDef, details);
        }

        [PublicAPI]
        public static PurchaseResult Cancelled => new PurchaseCancelled();

        [PublicAPI]
        public static PurchaseResult Failed(string message) {
            return new PurchaseFailed(message);
        }

        public sealed class PurchaseSucceed : PurchaseResult {
            public PurchaseDef                    PurchaseDef { get; }
            public SucceedPurchasePlatformDetails Details     { get; }

            internal PurchaseSucceed(PurchaseDef purchaseDef, SucceedPurchasePlatformDetails details) {
                this.PurchaseDef = purchaseDef;
                this.Details     = details;
            }
        }

        public sealed class PurchaseCancelled : PurchaseResult {
        }

        public sealed class PurchaseFailed : PurchaseResult {
            internal PurchaseFailed(string errorMessage) {
                this.ErrorMessage = errorMessage;
            }

            public string ErrorMessage { get; }
        }

        public abstract class SucceedPurchasePlatformDetails {
            public abstract PurchaseAnalyticsEvent BuildPurchaseEvent();
        }
    }
}