namespace Game.UI.Views.Friends {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class FriendsPanelView : AutoView<IFriendsPanelState> {
        [SerializeField, Required] private ViewPanel friendsListPanel;

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
            this.Event("open_incoming", () => this.State.OpenIncommingFriendships()),
            this.Event("open_add", () => this.State.OpenAddFriend()),
        };

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("requests_amount", () => this.State.RequestsAmount),
        };

        protected override void Render() {
            base.Render();

            this.friendsListPanel.Render(this.State.FriendsList);
        }
    }

    public interface IFriendsPanelState : IViewState {
        IState FriendsList { get; }

        int RequestsAmount { get; }

        void Close();
        void OpenIncommingFriendships();
        void OpenAddFriend();
    }
}