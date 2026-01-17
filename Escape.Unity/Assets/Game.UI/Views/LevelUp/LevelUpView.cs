namespace Game.UI.Views.LevelUp {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class LevelUpView : AutoView<ILevelUpState> {
        [SerializeField, Required] private ViewPanel rewards;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("prev_level", () => this.State.PrevLevel, 4),
            this.Variable("next_level", () => this.State.NextLevel, 5),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("continue", () => this.State.Continue()),
        };

        protected override void Render() {
            base.Render();

            this.rewards.Render(this.State.Rewards);
        }
    }

    public interface ILevelUpState : IViewState {
        int PrevLevel { get; }
        int NextLevel { get; }

        IState Rewards { get; }

        void Continue();
    }
}