namespace Game.UI.Views.QuestMenu {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class QuestMenuView : AutoView<IQuestMenuState> {
        [SerializeField, Required] private ViewPanel headerPanel;
        [SerializeField, Required] private ViewPanel questListPanel;
        [SerializeField, Required] private ViewPanel selectedQuest;

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };

        protected override void Render() {
            base.Render();

            this.headerPanel.Render(this.State.Header);
            this.questListPanel.Render(this.State.QuestList);
            this.selectedQuest.Render(this.State.SelectedQuest);
        }
    }

    public interface IQuestMenuState : IViewState {
        IState Header        { get; }
        IState QuestList     { get; }
        IState SelectedQuest { get; }

        void Close();
    }
}