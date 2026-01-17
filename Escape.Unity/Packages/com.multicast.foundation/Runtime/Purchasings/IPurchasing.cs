namespace Multicast.Purchasing {
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IPurchasing {
        [PublicAPI]
        PurchasingInitializationState InitializationState { get; }

        [PublicAPI]
        bool HasProduct(string purchaseKey);

        [PublicAPI]
        string GetLocalizedPriceString(string purchaseKey);

        string GetPurchaseKeyByStoreSpecificId(string storeSpecificId);

        [PublicAPI]
        UniTask<PurchaseResult> Purchase(string purchaseKey);

        [PublicAPI]
        UniTask<PurchasesRestoreResult> RestorePurchases();

        (string isoCurrencyCode, decimal localizedPrice) GetLocalizedPrice(string purchaseKey);
    }

    public enum PurchasingInitializationState {
        Loading,
        Initialized,
        InitializationFailed,
    }
}