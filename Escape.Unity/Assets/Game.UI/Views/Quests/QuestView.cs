namespace Game.UI.Views.Quests {
    using UniMob.UI;
    using Multicast;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class QuestView : AutoView<IQuestState> {
        [SerializeField, Required] private ViewPanel tasksPanel;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("quest_key", () => this.State.QuestKey, SharedConstants.Game.Quests.FIRST_STEPS),
        };

        protected override void Render() {
            base.Render();

            this.tasksPanel.Render(this.State.Tasks, link: true);
        }
    }

    public interface IQuestState : IViewState {
        string QuestKey { get; }

        IState Tasks { get; }
    }
}