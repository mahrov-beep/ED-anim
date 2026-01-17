namespace Game.UI.Views.MainMenu {
    using UniMob.UI;
    using Multicast;

    public class PartyMemberView : AutoView<IPartyMemberState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("name", () => this.State.Name, "Player_Name"),
            this.Variable("is_ready", () => this.State.IsReady, true),
            this.Variable("is_leader", () => this.State.IsLeader, true),
            this.Variable("local_is_leader", () => this.State.LocalIsLeader, true),
            this.Variable("is_in_menu", () => this.State.IsInMenu, true),
            this.Variable("is_in_game", () => this.State.IsInGame, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("make_party_leader", () => this.State.MakePartyLeader()),
            this.Event("kick", () => this.State.Kick()),
        };
    }

    public interface IPartyMemberState : IViewState {
        string Name { get; }

        bool IsReady       { get; }
        bool IsLeader      { get; }
        bool LocalIsLeader { get; }
        bool IsInMenu      { get; }
        bool IsInGame      { get; }

        void MakePartyLeader();
        void Kick();
    }
}