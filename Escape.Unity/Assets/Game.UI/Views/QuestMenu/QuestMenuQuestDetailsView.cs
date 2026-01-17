namespace Game.UI.Views.QuestMenu {
    using UniMob.UI;
    using Multicast;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class QuestMenuQuestDetailsView : AutoView<IQuestMenuQuestDetailsState> {
        [SerializeField, Required] private ViewPanel tasksPanel;
        [SerializeField, Required] private ViewPanel rewardsPanel;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("quest_key", () => this.State.QuestKey, SharedConstants.Game.Quests.FIRST_STEPS),
            this.Variable("is_completed", () => this.State.IsCompleted),
            this.Variable("can_reveal", () => this.State.CanReveal),
            this.Variable("can_claim", () => this.State.CanClaim, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("reveal", () => this.State.Reveal()),
            this.Event("claim", () => this.State.Claim()),
        };

        protected override void Render() {
            base.Render();

            this.tasksPanel.Render(this.State.Tasks);
            this.rewardsPanel.Render(this.State.Rewards);
        }
    }

    public interface IQuestMenuQuestDetailsState : IViewState {
        string QuestKey { get; }

        IState Tasks   { get; }
        IState Rewards { get; }

        bool IsCompleted { get; }

        bool CanReveal { get; }
        bool CanClaim  { get; }

        void Reveal();
        void Claim();
    }
}