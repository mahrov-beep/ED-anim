namespace Game.UI.Widgets.QuestMenu {
    using System.Linq;
    using Controllers.Features.Quest;
    using Domain.Quests;
    using Multicast;
    using Rewards;
    using Shared.UserProfile.Data.Quests;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.QuestMenu;

    [RequireFieldsInit]
    public class QuestMenuQuestDetailsWidget : StatefulWidget {
        public string QuestKey;
    }

    public class QuestMenuQuestDetailsState : ViewState<QuestMenuQuestDetailsWidget>, IQuestMenuQuestDetailsState {
        [Inject] private QuestsModel               questsModel;
        [Inject] private QuestCounterTasksModel    questCounterTasksModel;
        [Inject] private QuestDonateItemTasksModel questDonateItemTasksModel;

        private readonly StateHolder tasksState;
        private readonly StateHolder rewardsState;

        public QuestMenuQuestDetailsState() {
            this.tasksState   = this.CreateChild(this.BuildTasks);
            this.rewardsState = this.CreateChild(_ => new RewardsRowWidget { Rewards = this.Model.RewardsPreview });
        }

        public override WidgetViewReference View => UiConstants.Views.QuestMenu.QuestDetails;

        [Atom] private QuestModel Model => this.questsModel.Get(this.Widget.QuestKey);

        public string QuestKey    => this.Model.Key;
        public IState Tasks       => this.tasksState.Value;
        public IState Rewards     => this.rewardsState.Value;
        public bool   IsCompleted => this.Model.State is SdQuestStates.Completed;
        public bool   CanReveal   => this.Model.CanBeRevealed;
        public bool   CanClaim    => this.Model.CanBeCompleted;

        public void Reveal() {
            QuestFeatureEvents.RevealQuest.Raise(new QuestFeatureEvents.RevealQuestArgs {
                questKey = this.Model.Key,
            });
        }

        public void Claim() {
            QuestFeatureEvents.ClaimQuest.Raise(new QuestFeatureEvents.ClaimQuestArgs {
                questKey = this.Model.Key,
            });
        }

        private Widget BuildTasks(BuildContext buildContext) {
            return new ScrollGridFlow {
                MaxCrossAxisCount = 1,
                Padding           = new RectPadding(0, 0, 30, 100),
                Children = {
                    this.questCounterTasksModel.EnumerateForQuest(this.Model.Key).Select(it => this.BuildCounterTask(it)),
                    this.questDonateItemTasksModel.EnumerateForQuest(this.Model.Key).Select(it => this.BuildDonateItemTask(it)),
                },
            };
        }

        private Widget BuildCounterTask(QuestCounterTaskModel taskModel) {
            return new QuestMenuCounterTaskWidget {
                QuestCounterTaskKey = taskModel.Key,

                Key = Key.Of(taskModel.Key),
            };
        }

        private Widget BuildDonateItemTask(QuestDonateItemTaskModel taskModel) {
            return new QuestMenuDonateItemTaskWidget {
                QuestDonateItemTaskKey = taskModel.Key,

                Key = Key.Of(taskModel.Key),
            };
        }
    }
}