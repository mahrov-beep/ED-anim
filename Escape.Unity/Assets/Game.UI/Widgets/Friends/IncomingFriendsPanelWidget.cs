namespace Game.UI.Widgets.Friends {
    using System;
    using System.Linq;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Friends;

    [RequireFieldsInit]
    public class IncomingFriendsPanelWidget : StatefulWidget {
        public Action OnClose;

        public Action<string> OnAccept;
        public Action<string> OnDecline;

        public Action OnAcceptAll;
        public Action OnDeclineAll;

        public FriendsPanelState.FriendVM[] Requests;

        public string[] OnlineIds;
        public string[] InGameIds;
    }

    public class IncomingFriendsPanelState : ViewState<IncomingFriendsPanelWidget>, IIncomingFriendsPanelState {
        private readonly StateHolder requestsListState;

        public IncomingFriendsPanelState() {
            this.requestsListState = this.CreateChild(this.BuildRequestsList);
        }

        public override WidgetViewReference View => UiConstants.Views.Friends.Incoming;

        public IState RequestsList => this.requestsListState.Value;

        [Atom] public bool HasRequests => this.Widget.Requests is { Length: > 0 };

        private Widget BuildRequestsList(BuildContext context) {
            var scroll = new ScrollGridFlow {
                CrossAxisAlignment = CrossAxisAlignment.Center,
                MaxCrossAxisExtent = 1,
                Children = {
                    this.Widget.Requests.Select(vm => new FriendsListItemWidget {
                        UserId    = vm.UserId,
                        Nickname  = vm.Nickname,
                        IsRequest = true,
                        OnlineIds = this.Widget.OnlineIds,
                        InGameIds = this.Widget.InGameIds,
                        OnAccept  = this.AcceptFriend,
                        OnDecline = this.DeclineFriend,

                        Key = Key.Of(vm.UserId),
                    }),
                },
            };

            return scroll;
        }

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        private void AcceptFriend(string userId) {
            this.Widget.OnAccept?.Invoke(userId);

            if (this.Widget.Requests.Length == 1) {
                this.Close();
            }
        }

        private void DeclineFriend(string userId) {
            this.Widget.OnDecline?.Invoke(userId);
            
            if (this.Widget.Requests.Length == 1) {
                this.Close();
            }
        }

        public void AcceptAll() {
            this.Widget.OnAcceptAll?.Invoke();
            
            this.Close();
        }

        public void DeclineAll() {
            this.Widget.OnDeclineAll?.Invoke();
            
            this.Close();
        }
    }
}