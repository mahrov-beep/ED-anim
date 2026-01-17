namespace Game.UI.Widgets.Quests {
    using System.Linq;
    using Domain.Quests;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Quests;

    [RequireFieldsInit]
    public class QuestWidget : StatefulWidget {
        public string QuestKey;
    }

    public class QuestState : ViewState<QuestWidget>, IQuestState {
        [Inject] private QuestsModel               questsModel;
        [Inject] private QuestCounterTasksModel    questCounterTasksModel;
        [Inject] private QuestDonateItemTasksModel questDonateItemTasksModel;

        private readonly StateHolder tasksState;

        [Atom] private QuestModel Model => this.questsModel.Get(this.Widget.QuestKey);

        public QuestState() {
            this.tasksState = this.CreateChild(this.BuildTasks);
        }

        public override WidgetViewReference View => UiConstants.Views.Quests.Quest;

        public string QuestKey => this.Model.Key;
        public IState Tasks    => this.tasksState.Value;

        public override WidgetSize CalculateSize() {
            return WidgetSize.StackY(base.CalculateSize(), this.tasksState.Value.Size);
        }

        private Widget BuildTasks(BuildContext context) {
            return new Column {
                Children = {
                    this.questCounterTasksModel.EnumerateForQuest(this.Model.Key).Select(this.BuildQuestCounterTask),
                    this.questDonateItemTasksModel.EnumerateForQuest(this.Model.Key).Select(this.BuildQuestDonateItemTask),
                },
            };
        }

        private Widget BuildQuestCounterTask(QuestCounterTaskModel taskModel) {
            return new QuestCounterTaskWidget {
                QuestCounterTaskKey = taskModel.Key,
            };
        }

        private Widget BuildQuestDonateItemTask(QuestDonateItemTaskModel taskModel) {
            return new QuestDonateItemTaskWidget {
                QuestDonateItemTaskKey = taskModel.Key,
            };
        }
    }
}