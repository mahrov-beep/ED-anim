namespace Game.UI.Views.MainMenu {
    using UniMob.UI;
    using Multicast;

    public class MainMenuLevelView : AutoView<IMainMenuLevelState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("nickname", () => this.State.NickName, "Player#000000"),
            this.Variable("level", () => this.State.Level, 5),
            this.Variable("current_exp", () => this.State.CurrentExp, 80),
            this.Variable("total_exp", () => this.State.TotalExp, 100),
            this.Variable("rating", () => this.State.Rating, 99999),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("edit_nickname", () => this.State.EditNickName()),
        };
    }

    public interface IMainMenuLevelState : IViewState {
        string NickName { get; }

        int Level { get; }

        int CurrentExp { get; }
        int TotalExp   { get; }

        int Rating { get; }

        void EditNickName();
    }
}