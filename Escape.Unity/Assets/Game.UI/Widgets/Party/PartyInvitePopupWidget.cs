namespace Game.UI.Widgets.Party {
    using System;
    using Common;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    public class PartyInvitePopupWidget : StatefulWidget {
        public string LeaderUserId { get; }
        public string LeaderName   { get; }

        public Action<bool> OnResult { get; set; }

        public PartyInvitePopupWidget(string leaderUserId, string leaderName) {
            LeaderUserId = leaderUserId;
            LeaderName   = leaderName;
        }
    }

    public class PartyInvitePopupState : HocState<PartyInvitePopupWidget> {
        public override Widget Build(BuildContext context) {
            return AlertDialogWidget.YesNo("PARTY_INVITE").WithArgs(this.Widget.LeaderName);
        }
    }
}

