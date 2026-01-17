namespace Multicast.Purchasing {
    using JetBrains.Annotations;

    public abstract class PurchasesRestoreResult {
        [PublicAPI]
        public static PurchasesRestoreResult Restored(int restoredPurchases) {
            return new PurchasesRestored(restoredPurchases);
        }

        [PublicAPI]
        public static PurchasesRestoreResult Failed(string errorMessage) {
            return new PurchasesRestoreFailed(errorMessage);
        }

        public sealed class PurchasesRestored : PurchasesRestoreResult {
            public int RestoredPurchases { get; }

            internal PurchasesRestored(int restoredPurchases) {
                this.RestoredPurchases = restoredPurchases;
            }
        }

        public sealed class PurchasesRestoreFailed : PurchasesRestoreResult {
            public string ErrorMessage { get; }

            internal PurchasesRestoreFailed(string errorMessage) {
                this.ErrorMessage = errorMessage;
            }
        }
    }
}