#if UNITY_PURCHASING
namespace Multicast.Modules.Purchasing.UnityIAP {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Multicast.Analytics;
    using Collections;
    using Multicast.Purchasing;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;

    public class UnityIapPurchasing : IUnityIapValidationsRegistration, IPurchasing, ILifetimeScope, IStoreListener {
        [Atom] public PurchasingInitializationState InitializationState => this.state.Value;

        public Lifetime Lifetime => this.lifetime;

        private readonly Dictionary<string, UniTaskCompletionSource<PurchaseResult>> purchaseCompletions;

        private readonly MutableAtom<PurchasingInitializationState> state = Atom.Value(PurchasingInitializationState.Loading);

        private readonly Lifetime                          lifetime;
        private readonly LookupCollection<PurchaseDef>     purchaseDefs;
        private readonly IAnalytics                        analytics;
        private readonly List<IUnityIapValidationProvider> validationProviders;

        private IStoreController              storeController;
        private IAppleExtensions              appleExtensions;
        private IGooglePlayStoreExtensions    googleExtension;
        private ITransactionHistoryExtensions transactionHistoryExtensions;


        public UnityIapPurchasing(Lifetime lifetime, LookupCollection<PurchaseDef> purchaseDefs, IAnalytics analytics) {
            this.lifetime            = lifetime;
            this.purchaseDefs        = purchaseDefs;
            this.analytics           = analytics;
            this.validationProviders = new List<IUnityIapValidationProvider>();
            this.purchaseCompletions = new Dictionary<string, UniTaskCompletionSource<PurchaseResult>>();
        }

        public void Initialize() {
            if (this.storeController != null) {
                throw new InvalidOperationException("Already initialized.");
            }

            var module  = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);

            foreach (var productDef in this.purchaseDefs.Items) {
                var type = productDef.type switch {
                    PurchaseDef.ProductType.NonConsumable => ProductType.NonConsumable,
                    PurchaseDef.ProductType.Consumable => ProductType.Consumable,
                    PurchaseDef.ProductType.Subscription => ProductType.Subscription,
                    _ => throw new ArgumentException($"Unknown product type {productDef.type}")
                };

                builder.AddProduct(productDef.key, type, new IDs {
                    {productDef.androidID, GooglePlay.Name},
                    {productDef.iosID, AppleAppStore.Name},
                });
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void RegisterValidator(IUnityIapValidationProvider validationProvider) {
            this.validationProviders.Add(validationProvider);
        }

        public string GetPurchaseKeyByStoreSpecificId(string storeSpecificId) {
            var product = this.storeController.products.WithStoreSpecificID(storeSpecificId);

            if (product == null) {
                Debug.LogError($"Purchase with store id {storeSpecificId} not exist");
                return null;
            }

            return product.definition.id;
        }

        public bool HasProduct(string purchaseKey) {
            if (this.state.Value != PurchasingInitializationState.Initialized) {
                return false;
            }

            if (!this.purchaseDefs.TryGet(purchaseKey, out var purchase)) {
                return false;
            }

            return this.TryGetProduct(purchaseKey, out var product) && this.HasProduct(product);
        }

        public string GetLocalizedPriceString(string purchaseKey) {
            if (this.state.Value == PurchasingInitializationState.Loading) {
                return "...";
            }

            if (!this.TryGetProduct(purchaseKey, out var product)) {
                return "...";
            }

            var isoCurrencyCode = product?.metadata?.isoCurrencyCode?.ToUpper();
            if (isoCurrencyCode == null) {
                return null;
            }

            string price = null;

            if ("USD".Equals(isoCurrencyCode, StringComparison.InvariantCulture)) {
                price = product.metadata.localizedPriceString;
            }

            return price ?? $"{product.metadata.localizedPrice} {isoCurrencyCode}";
        }

        public async UniTask<PurchaseResult> Purchase(string purchaseKey) {
            if (this.state.Value != PurchasingInitializationState.Initialized) {
                return PurchaseResult.Failed("Store not initialized");
            }

            if (!this.TryGetProduct(purchaseKey, out var product)) {
                return PurchaseResult.Failed("Product not found");
            }

            if (!product.availableToPurchase) {
                return PurchaseResult.Failed("Product not available");
            }

            if (this.purchaseCompletions.ContainsKey(purchaseKey)) {
                return PurchaseResult.Cancelled;
            }

            Debug.Log($"[UnityIAP] Purchase initiated: {product.definition.id}");

            var completer = new UniTaskCompletionSource<PurchaseResult>();
            this.purchaseCompletions.Add(purchaseKey, completer);

            this.storeController.InitiatePurchase(product);

            var result = await completer.Task;

            return result;
        }

        public (string, decimal) GetLocalizedPrice(string purchaseKey) {
            if (this.TryGetProduct(purchaseKey, out var product)) {
                return (product.metadata.isoCurrencyCode, product.metadata.localizedPrice);
            }

            return ("USD", 0);
        }

        public async UniTask<PurchasesRestoreResult> RestorePurchases() {
            if (this.InitializationState != PurchasingInitializationState.Initialized) {
                return PurchasesRestoreResult.Failed("Store not initialized");
            }

            var completer = new UniTaskCompletionSource<bool>();

            switch (Application.platform) {
                case RuntimePlatform.IPhonePlayer:
                    this.appleExtensions.RestoreTransactions(result => completer.TrySetResult(result));
                    break;

                case RuntimePlatform.Android:
                    this.googleExtension.RestoreTransactions(result => completer.TrySetResult(result));
                    break;

                default:
                    completer.TrySetResult(true);
                    break;
            }

            var succeeded = await completer.Task;
            if (!succeeded) {
                return PurchasesRestoreResult.Failed("Restore request failed");
            }

            int productsOwned = 0;
            foreach (var product in this.storeController.products.all) {
                if (this.HasProduct(product)) {
                    productsOwned += 1;
                }
            }

            return PurchasesRestoreResult.Restored(productsOwned);
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error) {
            ((IStoreListener) this).OnInitializeFailed(error, "unset");
        }
        
        void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message) {
            this.state.Value = PurchasingInitializationState.InitializationFailed;

            Debug.Log($"$[UnityIAP] Initialization failed: {error}: {message}");

            this.analytics.Send(new UnityIapInitializationFailed(error.ToString(), message));

            this.state.Invalidate();
        }

        PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs purchaseEvent) {
            var product = purchaseEvent.purchasedProduct;

            this.HandlePurchase(product);

            return PurchaseProcessingResult.Complete;
        }

        private void HandlePurchase(Product product) {
            var productId = product.definition.id;

            if (!this.purchaseCompletions.TryGetValue(productId, out var completer)) {
                if (product.definition.type == ProductType.Consumable) {
                    Debug.LogError($"[UnityIAP] Consumable purchase succeed without completer: {productId}");
                }

                return;
            }

            this.purchaseCompletions.Remove(productId);

            if (!this.purchaseDefs.TryGet(product.definition.id, out var productDef)) {
                Debug.LogError("[UnityIAP] Purchase not exist");
                return;
            }

            var errorMessage = this.ValidateReceipt(product);

            if (errorMessage == null) {
                Debug.LogFormat("[UnityIAP] Purchase completed: {0}", product.definition.id);
            }
            else {
                Debug.LogFormat("[UnityIAP] Purchase failed: {0}, error={1}", product.definition.id, errorMessage);
            }

            completer.TrySetResult(errorMessage == null
                ? this.BuildSucceedPurchaseDetails(product, productDef)
                : PurchaseResult.Failed(errorMessage)
            );

            this.state.Invalidate();
        }

        private string ValidateReceipt(Product purchasedProduct) {
            try {
                foreach (var validator in this.validationProviders) {
                    var validatorErrorMessage = validator.Validate(purchasedProduct);

                    if (!string.IsNullOrEmpty(validatorErrorMessage)) {
                        return validatorErrorMessage;
                    }
                }

                return null;
            }
            catch (Exception ex) {
                Debug.LogError($"UnityIapPurchasing ValidateReceipt error: {ex.Message}");
                return null;
            }
        }

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
            var productId = product.definition.id;

            if (failureReason != PurchaseFailureReason.UserCancelled) {
                Debug.LogErrorFormat("[UnityIAP] Purchase failed: reason={0}", failureReason.ToString());
            }

            if (!this.purchaseCompletions.TryGetValue(productId, out var completer)) {
                Debug.LogError($"[UnityIAP] Purchase failed without completer: {productId}");
                return;
            }

            this.purchaseCompletions.Remove(productId);

            var storeErrorCode = string.Empty;
            var storeErrorMsg  = string.Empty;

            try {
                storeErrorCode = this.transactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode().ToString();
                storeErrorMsg  = this.transactionHistoryExtensions.GetLastPurchaseFailureDescription()?.message;
            }
            catch (Exception) {
                //
            }

            this.analytics.Send(new UnityIapPurchaseFailed(productId, storeErrorMsg, storeErrorCode, failureReason.ToString()));

            if (failureReason == PurchaseFailureReason.UserCancelled) {
                completer.TrySetResult(PurchaseResult.Cancelled);
            }
            else {
                completer.TrySetResult(PurchaseResult.Failed(failureReason.ToString()));
            }
        }

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            this.storeController              = controller;
            this.appleExtensions              = extensions.GetExtension<IAppleExtensions>();
            this.googleExtension              = extensions.GetExtension<IGooglePlayStoreExtensions>();
            this.transactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();

            this.state.Value = PurchasingInitializationState.Initialized;

            Debug.Log("[UnityIAP] Initialized");

            this.analytics.Send(new UnityIapInitializationSucceed());
        }

        private PurchaseResult BuildSucceedPurchaseDetails(Product product, PurchaseDef purchaseDef) => Application.platform switch {
            RuntimePlatform.Android => new PurchaseResult.PurchaseSucceed(purchaseDef, new GooglePlaySucceedPurchaseDetails(purchaseDef, product)),
            RuntimePlatform.IPhonePlayer => new PurchaseResult.PurchaseSucceed(purchaseDef, new AppStoreSucceedPurchaseDetails(purchaseDef, product)),
            _ => new PurchaseResult.PurchaseSucceed(purchaseDef, new SucceedPurchaseDetails(purchaseDef)),
        };

        private bool TryGetProduct(string productId, out Product product) {
            if (this.state.Value != PurchasingInitializationState.Initialized) {
                product = default;
                return false;
            }

            product = this.storeController.products.WithID(productId);
            return product != null;
        }

        private bool HasProduct(Product product) {
            if (product == null) {
                return false;
            }

            switch (product.definition.type) {
                case ProductType.Consumable:
                    return false;
                case ProductType.NonConsumable:
                    return product.hasReceipt;
                case ProductType.Subscription:
                    if (!product.hasReceipt) {
                        return false;
                    }

                    try {
                        var introductoryInfoDict = this.appleExtensions.GetIntroductoryPriceDictionary();

                        if (!CheckIfProductIsAvailableForSubscriptionManager(product.receipt)) {
                            Debug.LogWarning("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
                            return false;
                        }

                        var introJson = (introductoryInfoDict == null || !introductoryInfoDict.ContainsKey(product.definition.storeSpecificId))
                            ? null
                            : introductoryInfoDict[product.definition.storeSpecificId];
                        var p    = new SubscriptionManager(product, introJson);
                        var info = p.getSubscriptionInfo();

                        return info.isSubscribed() == Result.True;
                    }
                    catch (Exception e) {
                        Debug.LogException(e);
                        return false;
                    }

                default:
                    return false;
            }
        }

        private static bool CheckIfProductIsAvailableForSubscriptionManager(string receipt) {
            var receiptWrapper = (Dictionary<string, object>) MiniJson.JsonDecode(receipt);
            if (!receiptWrapper.ContainsKey("Store") || !receiptWrapper.ContainsKey("Payload")) {
                Debug.LogWarning("The product receipt does not contain enough information");
                return false;
            }

            var store   = (string) receiptWrapper["Store"];
            var payload = (string) receiptWrapper["Payload"];

            if (payload != null) {
                switch (store) {
                    case GooglePlay.Name: {
                        var payloadWrapper = (Dictionary<string, object>) MiniJson.JsonDecode(payload);
                        if (!payloadWrapper.ContainsKey("json")) {
                            Debug.LogWarning("The product receipt does not contain enough information, the 'json' field is missing");
                            return false;
                        }

                        return true;
                    }
                    case AppleAppStore.Name:
                    case AmazonApps.Name:
                    case MacAppStore.Name: {
                        return true;
                    }
                    default: {
                        return false;
                    }
                }
            }

            return false;
        }
    }
}
#endif