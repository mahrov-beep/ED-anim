namespace Game.UI.Views.QuestMenu {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class QuestMenuCounterTaskView : AutoView<IQuestMenuCounterTaskState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("task_key", () => this.State.TaskKey, SharedConstants.Game.QuestCounterTasks.PLAY_GAME),
            this.Variable("current_value", () => this.State.CurrentValue, 2),
            this.Variable("total_value", () => this.State.TotalValue, 10),
        };
    }

    public interface IQuestMenuCounterTaskState : IViewState {
        string TaskKey { get; }

        int CurrentValue { get; }
        int TotalValue   { get; }
    }
}