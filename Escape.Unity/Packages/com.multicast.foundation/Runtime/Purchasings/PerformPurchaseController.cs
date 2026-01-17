namespace Multicast.Purchasing {
    using System;
    using System.Collections.Generic;
    using Analytics;
    using Collections;
    using Cysharp.Threading.Tasks;
    using DropSystem;
    using Multicast;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct PerformPurchaseControllerArgs : IResultControllerArgs {
        public string     PurchaseKey;
        public string     ItemKey;
        public List<Drop> Drops;
    }

    [SkipInstallWithoutDependency(typeof(IPurchasing))]
    public class PerformPurchaseController : ResultController<PerformPurchaseControllerArgs> {
        [Inject] private readonly IPurchasing                   purchasing;
        [Inject] private readonly IAnalytics                    analytics;
        [Inject] private readonly LookupCollection<PurchaseDef> purchaseDefs;

        protected override async UniTask Execute(Context context) {
            if (!this.purchaseDefs.TryGet(this.Args.PurchaseKey, out var purchaseDef)) {
                throw new InvalidOperationException("Purchase not exist");
            }

            this.analytics.Send(new PurchaseInitiatedAnalyticsEvent {
                PurchaseKey = this.Args.PurchaseKey,
            });

            var (isoCurrencyCode, localizedPrice) = this.purchasing.GetLocalizedPrice(this.Args.PurchaseKey);

            var purchaseGuid = context.Execute(new InitiatePurchaseCommand {
                purchaseKey       = this.Args.PurchaseKey,
                itemKey           = this.Args.ItemKey,
                priceCents        = purchaseDef.priceUdsCents,
                iapCurrencyCode   = isoCurrencyCode,
                iapCurrencyAmount = Convert.ToDouble(localizedPrice),
                drops             = this.Args.Drops,
            }, default(string));

            var result = await this.purchasing.Purchase(this.Args.PurchaseKey);

            this.analytics.Send(new PurchaseEndAnalyticsEvent {
                PurchaseKey = this.Args.PurchaseKey,
            });

            switch (result) {
                case PurchaseResult.PurchaseSucceed purchaseSucceed:
                    var purchaseEvent = purchaseSucceed.Details.BuildPurchaseEvent();

                    context.Execute(new CompletePurchaseCommand {
                        purchaseGuid  = purchaseGuid,
                        transactionId = purchaseEvent.TransactionID,
                    });

                    this.analytics.Send(purchaseEvent);
                    break;

                case PurchaseResult.PurchaseCancelled:
                    context.Execute(new CancelPurchaseCommand {
                        purchaseGuid = purchaseGuid,
                    });

                    this.analytics.Send(new PurchaseCancelledAnalyticsEvent {
                        PurchaseKey = this.Args.PurchaseKey,
                    });
                    break;

                case PurchaseResult.PurchaseFailed purchaseFailed:
                    context.Execute(new FailPurchaseCommand {
                        purchaseGuid = purchaseGuid,
                        failMessage  = purchaseFailed.ErrorMessage,
                    });

                    this.analytics.Send(new PurchaseFailedAnalyticsEvent {
                        PurchaseKey  = this.Args.PurchaseKey,
                        ErrorMessage = purchaseFailed.ErrorMessage,
                    });
                    break;

                default:
                    Debug.LogError($"Unexpected purchase result {result?.GetType().Name}");
                    break;
            }
        }
    }
}