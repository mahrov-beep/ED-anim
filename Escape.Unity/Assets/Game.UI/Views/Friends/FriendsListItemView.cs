namespace Game.UI.Views.Friends {
    using UniMob.UI;
    using Multicast;
    using UnityEngine;
    using UnityEngine.UI;

    public class FriendsListItemView : AutoView<IFriendsListItemState> {
        [SerializeField] Image portrait;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("user_id", () => this.State.UserId, string.Empty), // ?????/
            this.Variable("nickname", () => this.State.Nickname, "AshotHeadshot"),
            this.Variable("status", () => this.State.IsInGame ? "In Game" : this.State.IsInMenu ? "In Menu" : "Offline", "Offline"),
            this.Variable("is_request", () => this.State.IsFriendshipRequest),
            this.Variable("can_party_invite", () => !this.State.IsFriendshipRequest && this.State.IsInMenu),
            this.Variable("is_in_party", () => this.State.IsInParty),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("invite", () => this.State.Invite()),
            this.Event("accept", () => this.State.Accept()),
            this.Event("decline", () => this.State.Decline()),
            this.Event("remove", () => this.State.Remove()),
        };

        protected override void Render() {
            base.Render();
            var avatar = this.State.Avatar;
            if (portrait.sprite != avatar) {
                portrait.sprite = avatar;
            }
        }
    }

    public interface IFriendsListItemState : IViewState {
        Sprite Avatar   { get; }
        string UserId   { get; }
        string Nickname { get; }

        bool IsFriendshipRequest { get; }
        bool IsInMenu  { get; }
        bool IsInGame  { get; }

        bool IsInParty { get; }

        void Invite();
        void Accept();
        void Decline();
        void Remove();
    }
}