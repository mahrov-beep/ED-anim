namespace Game.UI.Views.MainMenu {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class PartyStatusView : AutoView<IPartyStatusState> {
        [SerializeField, Required] private ViewPanel partyMembers;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("name", () => this.State.Name, "My_Name"),
            this.Variable("is_leader", () => this.State.IsLeader, true),
            this.Variable("is_ready", () => this.State.IsReady, true),
            this.Variable("is_member_party", () => this.State.IsMemberParty, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("toggle_ready", () => this.State.ToggleReady()),
            this.Event("leave", () => this.State.Leave()),
        };

        protected override void Render() {
            base.Render();

            this.partyMembers.Render(this.State.PartyMembers);
        }
    }

    public interface IPartyStatusState : IViewState {
        IState PartyMembers { get; }

        string Name { get; }

        bool IsLeader      { get; }
        bool IsReady       { get; }
        bool IsMemberParty { get; }

        void ToggleReady();
        void Leave();
    }
}