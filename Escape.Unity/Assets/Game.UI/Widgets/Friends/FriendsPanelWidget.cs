namespace Game.UI.Widgets.Friends {
    using System;
    using System.Linq;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Views;
    using Views.Friends;

    [RequireFieldsInit]
    public class FriendsPanelWidget : StatefulWidget {
        public Action OnClose;

        public Action         OnOpenIncoming;
        public Action         OnOpenAdd;
        public Action<string> OnInvite;
        public Action<string> OnRemove;

        public FriendsPanelState.FriendVM[] Friends;

        public int RequestsAmount;
        
        public string[] OnlineIds;
        public string[] InGameIds;
    }

    public class FriendsPanelState : ViewState<FriendsPanelWidget>, IFriendsPanelState {
        private readonly StateHolder friendsListState;

        public FriendsPanelState() {
            this.friendsListState = this.CreateChild(this.BuildFriendsList);
        }

        public override WidgetViewReference View => UiConstants.Views.Friends.Panel;

        public IState FriendsList => this.friendsListState.Value;

        public int RequestsAmount => this.Widget.RequestsAmount;

        private Widget BuildFriendsList(BuildContext context) {
            var scroll = new ScrollGridFlow {
                MaxCrossAxisExtent = 1,
                CrossAxisAlignment = CrossAxisAlignment.Center,
                Children = {
                    this.Widget.Friends.Select(vm => new FriendsListItemWidget {
                        UserId    = vm.UserId,
                        Nickname  = vm.Nickname,
                        IsRequest = false,
                        OnlineIds = this.Widget.OnlineIds,
                        InGameIds = this.Widget.InGameIds,
                        OnInvite  = this.InviteFriend,
                        OnRemove  = this.RemoveFriend,

                        Key = Key.Of(vm.UserId),
                    }),
                },
            };

            return scroll;
        }

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        public void OpenIncommingFriendships() {
            this.Widget.OnOpenIncoming?.Invoke();
        }

        public void OpenAddFriend() {
            this.Widget.OnOpenAdd?.Invoke();
        }

        private void RemoveFriend(string userId) {
            this.Widget.OnRemove?.Invoke(userId);
        }

        private void InviteFriend(string userId) {
            this.Widget.OnInvite?.Invoke(userId);
        }

        public readonly struct FriendVM {
            public readonly Guid   UserId;
            public readonly string Nickname;

            public FriendVM(Guid userId, string nickname) {
                this.UserId   = userId;
                this.Nickname = nickname;
            }
        }
    }
}