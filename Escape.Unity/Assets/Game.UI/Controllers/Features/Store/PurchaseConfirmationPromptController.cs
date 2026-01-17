namespace Game.UI.Controllers.Features.Store {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Multicast.Routes;
    using UniMob.UI.Widgets;
    using Widgets.Purchases;

    [Serializable, RequireFieldsInit]
    public struct PurchaseConfirmationPromptControllerArgs : IResultControllerArgs<bool> {
        public string storeItemKey;
    }

    public class PurchaseConfirmationPromptController : ResultController<PurchaseConfirmationPromptControllerArgs, bool> {
        protected override async UniTask<bool> Execute(Context context) {
            var confirmed = await context.RootNavigator.Push<bool>(new ScaleOutOverlayRoute(
                UiConstants.Views.BlackOverlay,
                new RouteSettings($"purchase_confirmation_{this.Args.storeItemKey}", RouteModalType.Popup),
                (buildContext, animation, secondaryAnimation) => new PurchasesConfirmationWidget(this.Args.storeItemKey) {
                    OnResult = result => context.RootNavigator.Pop(result),
                }
            ));

            return confirmed;
        }
    }
}