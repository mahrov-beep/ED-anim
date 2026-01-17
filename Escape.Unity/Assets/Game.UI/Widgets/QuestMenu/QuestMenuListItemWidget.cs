namespace Game.UI.Widgets.QuestMenu {
    using Domain.Quests;
    using Multicast;
    using Shared.UserProfile.Data.Quests;
    using UniMob;
    using UniMob.UI;
    using Views.QuestMenu;

    [RequireFieldsInit]
    public class QuestMenuListItemWidget : StatefulWidget {
        public string QuestKey;

        public MutableAtom<string> SelectedQuestKey;
    }

    public class QuestMenuListItemState : ViewState<QuestMenuListItemWidget>, IQuestMenuListItemState {
        [Inject] private QuestsModel questsModel;

        [Atom] private QuestModel Model => this.questsModel.Get(this.Widget.QuestKey);

        public override WidgetViewReference View => UiConstants.Views.QuestMenu.ListItem;

        public string QuestKey   => this.Model.Key;
        public int    Notifier   => this.Model.Notifier;
        public bool   IsSelected => this.Model.Key == this.Widget.SelectedQuestKey.Value;

        public bool IsCompleted    => this.Model.State is SdQuestStates.Completed;
        public bool CanBeCompleted => this.Model.CanBeCompleted;
        public bool IsRevealed     => this.Model.State is SdQuestStates.Revealed;
        public bool CanBeRevealed  => this.Model.CanBeRevealed;

        public void Select() {
            this.Widget.SelectedQuestKey.Value = this.Model.Key;
        }
    }
}