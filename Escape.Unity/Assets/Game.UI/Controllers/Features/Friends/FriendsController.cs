namespace Game.UI.Controllers.Features.Friends {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Shared.DTO;
    using Multicast;
    using Multicast.Routes;
    using UniMob;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.Friends;

    [Serializable, RequireFieldsInit]
    public struct FriendsControllerArgs : IFlowControllerArgs { }

    public class FriendsController : FlowController<FriendsControllerArgs> {
        private IUniTaskAsyncDisposable friendsScreen;
        private IUniTaskAsyncDisposable incomingScreen;
        private IUniTaskAsyncDisposable addFriendScreen;
        private IUniTaskAsyncDisposable bgScreen;
        private IUniTaskAsyncDisposable addFriendFade;
        private IUniTaskAsyncDisposable incomingFade;

        private MutableAtom<FriendsPanelState.FriendVM[]> friends;
        private MutableAtom<FriendsPanelState.FriendVM[]> requests;
        private MutableAtom<string[]>                     onlineIds;
        private MutableAtom<string[]>                     inGameIds;

        protected override async UniTask Activate(Context context) {
            friends   = Atom.Value(Lifetime, Array.Empty<FriendsPanelState.FriendVM>());
            requests  = Atom.Value(Lifetime, Array.Empty<FriendsPanelState.FriendVM>());
            onlineIds = Atom.Value(Lifetime, Array.Empty<string>());
            inGameIds = Atom.Value(Lifetime, Array.Empty<string>());

            App.Events.Listen<Shared.ServerEvents.FriendRequestIncomingAppServerEvent>(Lifetime, _ => {
                RequestFlow(UpdateFriends);
            });

            App.Events.Listen<Shared.ServerEvents.FriendAddedAppServerEvent>(Lifetime, _ => {
                RequestFlow(UpdateFriends);
            });

            App.Events.Listen<Shared.ServerEvents.FriendRemovedAppServerEvent>(Lifetime, _ => {
                RequestFlow(UpdateFriends);
            });

            App.Events.Listen<Shared.ServerEvents.FriendStatusChangedAppServerEvent>(Lifetime, evt => {
                RequestFlow(UpdateOnline, evt.FriendId.ToString());
            });

            await UpdateFriends(context);
            await RefreshOnline(context);
            await Open(context);
        }

        private async UniTask UpdateFriends(Context context) {
            await using (await context.RunProgressScreenDisposable("refreshing_friends_list")) {
                var friendsResponse =
                    await context.Server.Friends(
                        new FriendsListRequest() { },
                        ServerCallRetryStrategy.RetryWithUserDialog);

                var requestsResponse =
                    await context.Server.FriendsIncoming(
                        new IncomingRequestsRequest() { },
                        ServerCallRetryStrategy.RetryWithUserDialog);

                friends.Value = friendsResponse.Friends?.Select(x =>
                    new FriendsPanelState.FriendVM(x.Id, x.NickName)).ToArray();
                requests.Value = requestsResponse.IncomingRequests?.Select(x =>
                    new FriendsPanelState.FriendVM(x.Id, x.NickName)).ToArray();
            }
        }

        private async UniTask RefreshOnline(Context context) {
            var online   = await context.Server.FriendsOnline(new FriendsListRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
            var statuses = online.Friends ?? Array.Empty<FriendInfoDto>();
            onlineIds.Value = statuses.Where(f => f.Status == EUserStatus.InMenu).Select(f => f.Id.ToString()).ToArray();
            inGameIds.Value = statuses.Where(f => f.Status == EUserStatus.InGame).Select(f => f.Id.ToString()).ToArray();
        }

        private async UniTask UpdateOnline(Context context, string friendId) {
            await RefreshOnline(context);
        }

        private async UniTask Open(Context context) {
            bgScreen = await context.RunBgScreenDisposable();
            friendsScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.Friends,
                Page = () => new FriendsPanelWidget {
                    OnClose        = () => RequestFlow(Close),
                    OnOpenIncoming = () => RequestFlow(OpenIncoming),
                    OnOpenAdd      = () => RequestFlow(OpenAddFriend),
                    OnInvite       = userId => RequestFlow(InviteFriend, userId),
                    OnRemove       = userId => RequestFlow(RemoveFriend, userId),
                    Friends        = friends.Value,
                    OnlineIds      = onlineIds.Value,
                    InGameIds      = inGameIds.Value,
                    RequestsAmount = requests.Value.Length,
                },
            });

            if (requests.Value.Length > 0) {
                RequestFlow(OpenIncoming);
            }
        }

        private async UniTask AddFriendByNickname(Context context, string nickname) {
            var response = await context.Server.FriendsAddByNickname(new FriendAddByNicknameRequest {
                NickName = nickname,
            }, ServerCallRetryStrategy.RetryWithUserDialog);

            await CloseAddFriend(context);
        }

        private async UniTask InviteFriend(Context context, string userId) {
            if (Guid.TryParse(userId, out var id)) {
                await context.Server.PartyInvite(new PartyInviteRequest {
                    TargetUserId = id,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }
        }

        private async UniTask AcceptFriend(Context context, string userId) {
            if (Guid.TryParse(userId, out var id)) {
                await context.Server.FriendsAccept(new FriendAcceptRequest {
                    Id = id,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            await UpdateFriends(context);
        }

        private async UniTask DeclineFriend(Context context, string userId) {
            if (Guid.TryParse(userId, out var id)) {
                var response = await context.Server.FriendsDecline(new FriendDeclineRequest {
                    Id = id,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            await UpdateFriends(context);
        }

        private async UniTask RemoveFriend(Context context, string userId) {
            if (Guid.TryParse(userId, out var id)) {
                await context.Server.FriendsRemove(new FriendRemoveRequest {
                    Id = id,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            await UpdateFriends(context);
        }

        private async UniTask OpenIncoming(Context context) {
            incomingFade = await context.RunFadeScreenDisposable();
            incomingScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.FriendsIncoming,
                Page = () => new IncomingFriendsPanelWidget {
                    OnClose      = () => RequestFlow(CloseIncoming),
                    OnAccept     = userId => RequestFlow(AcceptFriend, userId),
                    OnDecline    = userId => RequestFlow(DeclineFriend, userId),
                    OnAcceptAll  = () => RequestFlow(AcceptAllIncoming),
                    OnDeclineAll = () => RequestFlow(DeclineAllIncoming),
                    Requests     = requests.Value,
                    OnlineIds    = onlineIds.Value,
                    InGameIds    = inGameIds.Value,
                },
            });
        }

        private async UniTask CloseIncoming(Context context) {
            await incomingScreen.DisposeAsync();
            if (incomingFade != null) {
                await incomingFade.DisposeAsync();
                incomingFade = null;
            }
        }

        private async UniTask AcceptAllIncoming(Context context) {
            var resp = await context.Server.FriendsIncomingBulk(new FriendsIncomingBulkRequest {
                Action = EFriendBulkAction.Accept,
            }, ServerCallRetryStrategy.RetryWithUserDialog);
            await UpdateFriends(context);
        }

        private async UniTask DeclineAllIncoming(Context context) {
            var resp = await context.Server.FriendsIncomingBulk(new FriendsIncomingBulkRequest {
                Action = EFriendBulkAction.Decline,
            }, ServerCallRetryStrategy.RetryWithUserDialog);
            await UpdateFriends(context);
        }

        private async UniTask OpenAddFriend(Context context) {
            addFriendFade = await context.RunFadeScreenDisposable();
            addFriendScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.FriendsAdd,
                Page = () => new AddFriendWidget {
                    OnSubmit = nickname => RequestFlow(AddFriendByNickname, nickname),
                    OnClose  = () => RequestFlow(CloseAddFriend),
                },
            });
        }

        private async UniTask CloseAddFriend(Context context) {
            await addFriendScreen.DisposeAsync();
            if (addFriendFade != null) {
                await addFriendFade.DisposeAsync();
                addFriendFade = null;
            }
        }

        private async UniTask Close(Context context) {
            await friendsScreen.DisposeAsync();
            if (bgScreen != null) {
                await bgScreen.DisposeAsync();
                bgScreen = null;
            }
            Stop();
        }
    }
}