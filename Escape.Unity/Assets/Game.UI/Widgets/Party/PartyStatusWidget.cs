namespace Game.UI.Widgets.Party {
    using System;
    using System.Linq;
    using Domain.Party;
    using Shared.DTO;
    using Multicast;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.MainMenu;

    public class PartyStatusWidget : StatefulWidget {
        public string[] OnlineIds { get; set; }
        public string[] InGameIds { get; set; }
    }

    public class PartyStatusState : ViewState<PartyStatusWidget>, IPartyStatusState {
        [Inject] private PartyModel party;

        private readonly StateHolder partyMembersState;
        
        public PartyStatusState() {
            this.partyMembersState = this.CreateChild(this.BuildContent);
        }

        public override WidgetViewReference View => UiConstants.Views.Party.PartyStatus;

        public IState PartyMembers => this.partyMembersState.Value;

        public string Name => App.ServerAccessTokenInfo.UserId.ToString();

        public bool IsLeader => this.party.IsLeader;
        public bool IsReady  => this.party.IsSelfReady || this.IsLeader;

        public bool IsMemberParty {
            get {
                var members = this.party.Members.Value;

                if (members == null || members.Length <= 1) {
                    return false;
                }
            
                var isMember = members.Contains(App.ServerAccessTokenInfo.UserId);

                if (!isMember) {
                    return false;
                }

                return true;
            }
        }
        
        public void ToggleReady() {
            this.ToggleReady(this.party.Leader.Value);
        }
        
        public void Leave() {
            this.Leave(this.party.Leader.Value);
        }

        private Widget BuildContent(BuildContext context) {
            var members = this.party.Members.Value;

            if (members == null || members.Length <= 1) {
                return new Empty();
            }
            
            var isMember = members.Contains(App.ServerAccessTokenInfo.UserId);

            if (!isMember) {
                return new Empty();
            }

            return new Column {
                MainAxisAlignment  = MainAxisAlignment.Start,
                CrossAxisAlignment = CrossAxisAlignment.End,
                Children = {
                    members
                        .Where(memberId => memberId != App.ServerAccessTokenInfo.UserId)
                        .Select(memberId => BuildMember(memberId, this.party.Leader.Value, this.party.IsLeader)),
                },
            };
        }

        private Widget BuildMember(Guid memberId, Guid leaderId, bool localIsLeader) {
            var isLeader = memberId == leaderId;
            var isReady  = this.party.ReadyMembers.Value.Contains(memberId);
            
            var isLeaderMark = isLeader ? " (L)" : string.Empty;
            var isReadyMark  = isReady ? " [R]" : string.Empty;

            return new PartyMemberWidget() {
                Kick            = () => Kick(leaderId, memberId),
                MakePartyLeader = () => MakeLeader(memberId),
                Name            = $"{memberId}{isLeaderMark}{isReadyMark}",
                IsLeader        = isLeader,
                IsReady         = isReady,
                LocalIsLeader   = localIsLeader,
                OnlineIds       = this.Widget.OnlineIds,
                InGameIds       = this.Widget.InGameIds,
                UserId          = memberId.ToString(),
            };
        }
        
        private async void Leave(Guid leaderId) {
            await App.Server.PartyLeave(new PartyLeaveRequest { LeaderUserId = leaderId }, ServerCallRetryStrategy.RetryWithUserDialog);
            
            try {
                var status = await App.Server.PartyStatus(new PartyStatusRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
                
                if (status.LeaderUserId == Guid.Empty || status.Members == null || status.Members.Length <= 1 || !status.Members.Contains(App.ServerAccessTokenInfo.UserId)) {
                    this.party.Clear();
                } 
                else {
                    this.party.Set(status.LeaderUserId, status.Members);
                }
            } catch (Exception) {
                this.party.Clear();
            }
        }

        private async void Kick(Guid leaderId, Guid memberId) {
            await App.Server.PartyKick(new PartyKickRequest { LeaderUserId = leaderId, TargetUserId = memberId }, ServerCallRetryStrategy.RetryWithUserDialog);
        }

        private async void MakeLeader(Guid memberId) {
            var leaderId = this.party.Leader.Value;
            await App.Server.PartyMakeLeader(new PartyMakeLeaderRequest { LeaderUserId = leaderId, TargetUserId = memberId }, ServerCallRetryStrategy.RetryWithUserDialog);
        }

        private async void ToggleReady(Guid leaderId) {
            var desired = !this.party.IsSelfReady;
            await App.Server.PartySetReady(new PartySetReadyRequest { LeaderUserId = leaderId, IsReady = desired }, ServerCallRetryStrategy.RetryWithUserDialog);
        }
    }
}


