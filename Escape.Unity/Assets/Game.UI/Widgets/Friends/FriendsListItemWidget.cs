namespace Game.UI.Widgets.Friends {
    using System;
    using System.Linq;
    using Domain.Party;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UnityEngine;
    using Views.Friends;

    public class FriendsListItemWidget : StatefulWidget {
        public Guid                  UserId;
        public string                Nickname;
        public bool                  IsRequest;
        
        public Action<string> OnAccept;
        public Action<string> OnDecline;

        public Action<string> OnInvite;
        public Action<string> OnRemove;
        
        public string[] OnlineIds;
        public string[] InGameIds;
    }

    public class FriendsListItemState : ViewState<FriendsListItemWidget>, IFriendsListItemState {
        [Inject] private PartyModel party;

        public override WidgetViewReference View => UiConstants.Views.Friends.ListItem;

        [Atom] public Sprite Avatar   { get; set; }
        public        string UserId   => this.Widget.UserId.ToString();
        [Atom] public string Nickname => this.Widget.Nickname;
        
        [Atom] public bool IsInMenu =>  this.Widget.OnlineIds != null && this.Widget.OnlineIds != null && this.Widget.OnlineIds.Contains(this.UserId);
        [Atom] public bool IsInGame => this.Widget.InGameIds != null && this.Widget.InGameIds != null && this.Widget.InGameIds.Contains(this.UserId);

        public bool IsInParty => this.party.Members.Value != null && this.party.Members.Value.Contains(this.Widget.UserId);

        public bool IsFriendshipRequest => this.Widget.IsRequest;

        public void Invite() {
            this.Widget.OnInvite?.Invoke(this.UserId);
        }

        public void Accept() {
            this.Widget.OnAccept?.Invoke(this.UserId);
        }

        public void Decline() {
            this.Widget.OnDecline?.Invoke(this.UserId);
        }

        public void Remove() {
            this.Widget.OnRemove?.Invoke(this.UserId);
        }
    }
}