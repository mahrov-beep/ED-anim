namespace Game.UI.Widgets.Quests {
    using Domain.Quests;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using Views.Quests;

    [RequireFieldsInit]
    public class QuestCounterTaskWidget : StatefulWidget {
        public string QuestCounterTaskKey;
    }

    public class QuestCounterTaskState : ViewState<QuestCounterTaskWidget>, IQuestCounterTaskState {
        [Inject] private QuestCounterTasksModel questCounterTasksModel;

        [Atom] private QuestCounterTaskModel Model => this.questCounterTasksModel.Get(this.Widget.QuestCounterTaskKey);

        public override WidgetViewReference View => this.Model.IsUnlocked
            ? UiConstants.Views.Quests.CounterTaskCompleted
            : UiConstants.Views.Quests.CounterTaskActive;

        public string TaskKey      => this.Model.Key;
        public int    CurrentValue => this.Model.CounterValue;
        public int    TotalValue   => this.Model.TotalValue;
    }
}