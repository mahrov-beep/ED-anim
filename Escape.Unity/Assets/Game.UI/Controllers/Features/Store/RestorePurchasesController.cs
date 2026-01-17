namespace Game.UI.Controllers.Features.Store {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.AnalyticEvents;
    using Multicast;
    using Multicast.Analytics;
    using Multicast.Purchasing;
    using Multicast.Routes;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.Subscription;

    [Serializable, RequireFieldsInit]
    public struct RestorePurchasesControllerArgs : IResultControllerArgs {
    }

    public class RestorePurchasesController : ResultController<RestorePurchasesControllerArgs> {
        [Inject] private IPurchasing purchasing;
        [Inject] private IAnalytics  analytics;

        protected override async UniTask Execute(Context context) {
            this.analytics.Send(new RestorePurchasesInitiatedAnalyticsEvent());

            var result = await this.purchasing.RestorePurchases();

            switch (result) {
                case PurchasesRestoreResult.PurchasesRestored purchaseSucceed:
                    this.analytics.Send(new RestorePurchasesSucceedEvent {
                        Count = purchaseSucceed.RestoredPurchases,
                    });

                    await context.RootNavigator.Push(new SlideDownOverlayRoute(
                        UiConstants.Views.BlackOverlay,
                        new RouteSettings("purchases_restored", RouteModalType.Popup),
                        (buildContext, animation, secondaryAnimation) => new RestoredPurchasesWidget {
                            OnClose = () => context.RootNavigator.Pop(),
                        }
                    )).WithPopOnBack(context.RootNavigator).PushTask;

                    break;

                case PurchasesRestoreResult.PurchasesRestoreFailed purchaseFailed:
                    this.analytics.Send(new RestorePurchasesFailedEvent {
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