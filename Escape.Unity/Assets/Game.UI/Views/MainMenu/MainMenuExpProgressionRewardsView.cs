namespace Game.UI.Views.MainMenu {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class MainMenuExpProgressionRewardsView : AutoView<IMainMenuExpProgressionRewardsState> {
        [SerializeField, Required] private ViewPanel rewards;

        // protected override AutoViewVariableBinding[] Variables => new[] {
        //     
        // };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("open", () => this.State.Open()),
        };

        protected override void Render() {
            base.Render();

            this.rewards.Render(this.State.Rewards);
        }
    }

    public interface IMainMenuExpProgressionRewardsState : IViewState {
        IState Rewards { get; }

        void Open();
    }
}