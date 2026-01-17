namespace Game.UI.Controllers.Features.Party {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Multicast.Routes;
    using UniMob.UI.Widgets;
    using Widgets.Common;
    using Widgets.Party;

    [Serializable, RequireFieldsInit]
    public struct PartyInviteControllerArgs : IResultControllerArgs<bool> {
        public string leaderUserId;
        public string leaderName;
    }

    public class PartyInviteController : ResultController<PartyInviteControllerArgs, bool> {
        protected override async UniTask<bool> Execute(Context context) {
            var result = await context.RootNavigator.Push<bool>(new SlideDownOverlayRoute(
                UiConstants.Views.BlackOverlay,
                UiConstants.Routes.PartyInvite,
                (buildContext, animation, secondaryAnimation) => new PartyInvitePopupWidget(this.Args.leaderUserId, this.Args.leaderName) {
                    OnResult = accepted => context.RootNavigator.Pop(accepted),
                }
            ));

            if (result) {
                await using (await context.RunProgressScreenDisposable("party_accept")) {
                    await context.Server.PartyAccept(new Game.Shared.DTO.PartyAcceptInviteRequest {
                        LeaderUserId = Guid.Parse(this.Args.leaderUserId),
                    }, ServerCallRetryStrategy.RetryWithUserDialog);
                }
            }
            else {
                await context.Server.PartyDecline(new Game.Shared.DTO.PartyDeclineInviteRequest {
                    LeaderUserId = Guid.Parse(this.Args.leaderUserId),
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            return result;
        }
    }
}

