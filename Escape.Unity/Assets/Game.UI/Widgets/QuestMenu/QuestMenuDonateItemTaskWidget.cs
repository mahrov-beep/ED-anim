namespace Game.UI.Widgets.QuestMenu {
    using Controllers.Features.Quest;
    using Domain.Quests;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using Views.QuestMenu;

    [RequireFieldsInit]
    public class QuestMenuDonateItemTaskWidget : StatefulWidget {
        public string QuestDonateItemTaskKey;
    }

    public class QuestMenuDonateItemTaskState : ViewState<QuestMenuDonateItemTaskWidget>, IQuestMenuDonateItemTaskState {
        [Inject] private QuestDonateItemTasksModel questDonateItemTasksModel;

        [Atom] private QuestDonateItemTaskModel Model => this.questDonateItemTasksModel.Get(this.Widget.QuestDonateItemTaskKey);

        public override WidgetViewReference View => 0 switch {
            _ when !this.Model.IsRevealed => UiConstants.Views.QuestMenu.DonateItemTaskLocked,
            _ when !this.Model.IsCompleted => UiConstants.Views.QuestMenu.DonateItemTaskActive,
            _ => UiConstants.Views.QuestMenu.DonateItemTaskDonated,
        };

        public string TaskKey      => this.Model.Key;
        public bool   IsDonated    => this.Model.IsCompleted;
        public bool   CanBeDonated => this.Model.CanBeDonated;

        public void Donate() {
            QuestFeatureEvents.DonateItem.Raise(new QuestFeatureEvents.DonateItemArgs {
                questDonateItemTaskKey = this.Model.Key,
            });
        }
    }
}