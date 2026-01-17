namespace Game.UI.Widgets.Party {
    using System;
    using System.Linq;
    using UniMob;
    using UniMob.UI;
    using Views.MainMenu;

    public class PartyMemberWidget : StatefulWidget {
        public Action Kick            { get; set; }
        public Action MakePartyLeader { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

        public bool IsReady       { get; set; }
        public bool IsLeader      { get; set; }
        public bool LocalIsLeader { get; set; }

        public string[] OnlineIds { get; set; }
        public string[] InGameIds { get; set; }
    }

    public class PartyMemberState : ViewState<PartyMemberWidget>, IPartyMemberState {
        public override WidgetViewReference View => UiConstants.Views.Party.PartyMember;

        public string Name => this.Widget.Name;
        
        public bool IsReady       => this.Widget.IsReady;
        public bool IsLeader      => this.Widget.IsLeader;
        public bool LocalIsLeader => this.Widget.LocalIsLeader;
        
        [Atom] public bool IsInMenu =>  this.Widget.OnlineIds != null && this.Widget.OnlineIds != null && this.Widget.OnlineIds.Contains(this.Widget.UserId);
        [Atom] public bool IsInGame => this.Widget.InGameIds != null && this.Widget.InGameIds != null && this.Widget.InGameIds.Contains(this.Widget.UserId);
        
        public void MakePartyLeader() {
            this.Widget.MakePartyLeader?.Invoke();
        }
        
        public void Kick() {
            this.Widget.Kick?.Invoke();
        }
    }
}


