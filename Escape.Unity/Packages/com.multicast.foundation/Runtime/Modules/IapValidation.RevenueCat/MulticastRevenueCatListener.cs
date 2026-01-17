#if REVENUE_CAT_SDK
namespace Multicast.Modules.IapValidation.RevenueCat {
    using Multicast.Analytics;
    using Scellecs.Morpeh;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using AdjustSdk;
    using Multicast.AdAchievements;
    using Multicast.Purchasing;

    public class MulticastRevenueCatListener : Purchases.UpdatedCustomerInfoListener {
        [SerializeField, Required] private Purchases purchases;

        private UdRevenueCatValidationRepo validationRepo;

        private IAnalytics  analytics;
        private IPurchasing purchasing;

        public double NextValidationTimeSeconds { get; set; }

        public Purchases Purchases => this.purchases;

        private bool AllowSandbox =>
#if REVENUE_CAT_ALLOW_SANDBOX
            true
#else
            false
#endif
        ;

        public void Initialize(IAnalytics analytics, UdRevenueCatValidationRepo validationRepo, IPurchasing purchasing) {
            this.analytics      = analytics;
            this.validationRepo = validationRepo;
            this.purchasing     = purchasing;

            DontDestroyOnLoad(this.gameObject);
            
            this.AllowValidationAfter(minutes: 0.1f);
        }

        public string Validate(string storeSpecificId, string receipt) {
            if (this.IsSubscription(storeSpecificId)) {
                App.Execute(RevenueCatUpdateSubscriptionValidationCommand.SetNewTrialReceipt(receipt));
            }

            App.Execute(new RevenueCatAddValidationPurchaseCommand(storeSpecificId, receipt));

            this.AllowValidationAfter(minutes: 1);

            return string.Empty;
        }

        public bool NeedToValidatePurchases() {
            if (Time.unscaledTimeAsDouble < this.NextValidationTimeSeconds) {
                return false;
            }

            if (this.purchasing.InitializationState != PurchasingInitializationState.Initialized) {
                return false;
            }

            return this.validationRepo.ReceiptsCount > 0 ||
                   !string.IsNullOrEmpty(this.validationRepo.ValidatedTrialReceipts);
        }

        public void ValidatePurchases() {
            this.AllowValidationAfter(minutes: 5);

            Debug.Log("$[MulticastRevenueCatListener] Validate purchases");

            this.purchases.SyncPurchases();
            this.purchases.CollectDeviceIdentifiers();
            this.purchases.GetCustomerInfo(this.ValidatePurchases);
        }

        private void AllowValidationAfter(float minutes) {
            const double minutesToSeconds = 60;
            this.NextValidationTimeSeconds = Time.unscaledTimeAsDouble + minutes * minutesToSeconds;
        }

        private void ValidatePurchases(Purchases.CustomerInfo info, Purchases.Error error) {
            foreach (var e in info.Entitlements.All) {
                var value = e.Value;

                var isSubscription           = this.IsSubscription(value.ProductIdentifier);
                var isNotTrial               = value.PeriodType.ToLower() != "trial";
                var hasNotBoughtSubscription = !string.IsNullOrEmpty(this.validationRepo.ValidatedTrialReceipts);
                var subscriptionNotSend      = value.LatestPurchaseDate.Ticks > this.validationRepo.LastSubscriptionSentTicks;

                if (isSubscription &&
                    isNotTrial &&
                    value.IsActive &&
                    (!value.IsSandbox || this.AllowSandbox) &&
                    hasNotBoughtSubscription &&
                    subscriptionNotSend) {
                    Debug.Log("Sub condition met!");

                    var purchaseKey = this.purchasing.GetPurchaseKeyByStoreSpecificId(value.ProductIdentifier);
                    var price       = this.purchasing.GetLocalizedPrice(purchaseKey);

                    this.analytics.Send(new IapRevenueAnalyticsEvent {
                        StoreSpecificId  = value.ProductIdentifier,
                        ValidatedReceipt = this.validationRepo.ValidatedTrialReceipts,
                        Currency         = price.isoCurrencyCode,
                        Price            = price.localizedPrice,
                    });

                    App.Execute(RevenueCatUpdateSubscriptionValidationCommand.SetSubscriptionTicks(
                        value.LatestPurchaseDate.Ticks));
                }
                else if (!isSubscription && (!value.IsSandbox || this.AllowSandbox)) {
                    if (this.validationRepo.HasPurchase(value.ProductIdentifier)) {
                        var firstReceipt = this.validationRepo.FirstByIdentifier(value.ProductIdentifier);

                        var purchaseKey = this.purchasing.GetPurchaseKeyByStoreSpecificId(value.ProductIdentifier);
                        var price       = this.purchasing.GetLocalizedPrice(purchaseKey);

                        this.analytics.Send(new IapRevenueAnalyticsEvent {
                            StoreSpecificId  = value.ProductIdentifier,
                            ValidatedReceipt = firstReceipt,
                            Currency         = price.isoCurrencyCode,
                            Price            = price.localizedPrice,
                        });

                        Debug.Log("Purchase Validated!");

                        App.Execute(new RevenueCatRemoveValidationPurchaseCommand(value.ProductIdentifier, firstReceipt));
                    }
                    else {
                        Debug.Log($"Can't Find: {value.ProductIdentifier}");
                    }
                }
                else {
                    Debug.Log($"Can't Validate: {value.ProductIdentifier}");
                }
            }

            if (error != null) {
                LogError(error);
            }
        }

        private bool IsSubscription(string productIdentifier) {
            return productIdentifier.Contains("week")
                   || productIdentifier.Contains("monthly")
                   || productIdentifier.Contains("year");
        }

        public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo) {
            foreach (var sub in customerInfo.ActiveSubscriptions) {
                Debug.LogWarning($"[RevenueCatValidation] Customer info received -> {sub}");
            }
        }

        private static void LogError(Purchases.Error error) {
            Debug.LogError(JsonUtility.ToJson(error));
        }
    }
}
#endif