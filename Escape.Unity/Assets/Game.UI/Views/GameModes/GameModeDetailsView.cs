namespace Game.UI.Views.GameModes {
    using UniMob.UI;
    using Multicast;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameModeDetailsView : AutoView<IGameModeDetailsState> {
        [SerializeField, Required] private ViewPanel lootDetailsPanel;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("game_mode_key", () => this.State.GameModeKey, SharedConstants.Game.GameModes.INIT_GAME_MODE),
            this.Variable("current_quality", () => this.State.CurrentQuality, 50),
            this.Variable("required_quality", () => this.State.RequiredQuality, 80),
            this.Variable("current_profile_level", () => this.State.CurrentProfileLevel, 1),
            this.Variable("required_profile_level", () => this.State.RequiredProfileLevel, 5),
        };

        protected override void Render() {
            base.Render();

            this.lootDetailsPanel.Render(this.State.LootDetails);
        }
    }

    public interface IGameModeDetailsState : IViewState {
        string GameModeKey { get; }

        int RequiredQuality { get; }
        int CurrentQuality  { get; }

        int RequiredProfileLevel { get; }
        int CurrentProfileLevel  { get; }

        IState LootDetails { get; }
    }
}