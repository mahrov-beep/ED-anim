namespace Game.UI.Views.QuestMenu {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class QuestMenuDonateItemTaskView : AutoView<IQuestMenuDonateItemTaskState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("task_key", () => this.State.TaskKey, SharedConstants.Game.QuestCounterTasks.PLAY_GAME),
            this.Variable("is_donated", () => this.State.IsDonated),
            this.Variable("can_be_donated", () => this.State.CanBeDonated, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("donate", () => this.State.Donate()),
        };
    }

    public interface IQuestMenuDonateItemTaskState : IViewState {
        string TaskKey { get; }

        bool IsDonated    { get; }
        bool CanBeDonated { get; }

        void Donate();
    }
}