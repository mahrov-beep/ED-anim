namespace Game.UI.Widgets.Quests {
    using Domain.Quests;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using Views.Quests;

    [RequireFieldsInit]
    public class QuestDonateItemTaskWidget : StatefulWidget {
        public string QuestDonateItemTaskKey;
    }

    public class QuestDonateItemTaskState : ViewState<QuestDonateItemTaskWidget>, IQuestDonateItemTaskState {
        [Inject] private QuestDonateItemTasksModel questDonateItemTasksModel;

        [Atom] private QuestDonateItemTaskModel Model => this.questDonateItemTasksModel.Get(this.Widget.QuestDonateItemTaskKey);

        public override WidgetViewReference View => 0 switch {
            _ when !this.Model.IsCompleted => UiConstants.Views.Quests.DonateItemTaskActive,
            _ => UiConstants.Views.Quests.DonateItemTaskCompleted,
        };

        public string TaskKey => this.Model.Key;
    }
}