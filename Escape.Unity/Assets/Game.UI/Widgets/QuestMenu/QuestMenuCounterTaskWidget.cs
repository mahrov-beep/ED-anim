namespace Game.UI.Widgets.QuestMenu {
    using Domain.Quests;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using Views.QuestMenu;

    [RequireFieldsInit]
    public class QuestMenuCounterTaskWidget : StatefulWidget {
        public string QuestCounterTaskKey;
    }

    public class QuestMenuCounterTaskState : ViewState<QuestMenuCounterTaskWidget>, IQuestMenuCounterTaskState {
        [Inject] private QuestCounterTasksModel questCounterTasksModel;

        [Atom] private QuestCounterTaskModel Model => this.questCounterTasksModel.Get(this.Widget.QuestCounterTaskKey);

        public override WidgetViewReference View => 0 switch {
            _ when !this.Model.IsRevealed => UiConstants.Views.QuestMenu.CounterTaskLocked,
            _ when !this.Model.IsUnlocked => UiConstants.Views.QuestMenu.CounterTaskActive,
            _ => UiConstants.Views.QuestMenu.CounterTaskCompleted,
        };

        public string TaskKey      => this.Model.Key;
        public int    CurrentValue => this.Model.CounterValue;
        public int    TotalValue   => this.Model.TotalValue;
    }
}