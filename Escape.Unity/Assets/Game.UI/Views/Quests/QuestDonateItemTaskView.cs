namespace Game.UI.Views.Quests {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class QuestDonateItemTaskView : AutoView<IQuestDonateItemTaskState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("task_key", () => this.State.TaskKey, SharedConstants.Game.QuestCounterTasks.PLAY_GAME),
        };
    }

    public interface IQuestDonateItemTaskState : IViewState {
        string TaskKey { get; }
    }
}