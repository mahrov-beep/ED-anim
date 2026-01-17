namespace Game.UI.Views.ExpProgressionRewards {
    using UniMob.UI;
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class ExpProgressionRewardsScreenView : AutoView<IExpProgressionRewardsScreenState> {
        [SerializeField, Required] private ViewPanel rewards;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("selected_title", () => this.State.SelectedTitle, "AK-47"),
            this.Variable("selected_desc", () => this.State.SelectedDesc, "Description"),
            this.Variable("selected_rarity", () => this.State.SelectedRarity, ERarityType.Legendary),
            this.Variable("selected_locked", () => this.State.SelectedIsLocked, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };

        protected override void Render() {
            base.Render();

            this.rewards.Render(this.State.Rewards);
        }
    }

    public interface IExpProgressionRewardsScreenState : IViewState {
        string SelectedTitle    { get; }
        string SelectedDesc     { get; }
        string SelectedRarity   { get; }
        bool   SelectedIsLocked { get; }

        IState Rewards { get; }

        void Close();
    }
}