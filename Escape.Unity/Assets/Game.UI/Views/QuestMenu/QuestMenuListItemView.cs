namespace Game.UI.Views.QuestMenu {
    using UniMob.UI;
    using Multicast;
    using Shared;
    using UnityEngine;

    public class QuestMenuListItemView : AutoView<IQuestMenuListItemState> {
        [SerializeField] private GameObject lockBlocker;
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("quest_key", () => this.State.QuestKey, SharedConstants.Game.Quests.FIRST_STEPS),
            this.Variable("notifier", () => this.State.Notifier, 1),

            this.Variable("is_selected", () => this.State.IsSelected),
            this.Variable("is_completed", () => this.State.IsCompleted),
            this.Variable("can_be_completed", () => this.State.CanBeCompleted),
            this.Variable("is_revealed", () => this.State.IsRevealed),
            this.Variable("can-be_revealed", () => this.State.CanBeRevealed),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("select", () => this.State.Select()),
        };

        protected override void Render() {
            base.Render();
            if (State.IsCompleted || State.IsRevealed) {
                lockBlocker.SetActive(false);
            }
            else {
                this.lockBlocker.SetActive(!State.CanBeRevealed);
            }
        }
    }

    public interface IQuestMenuListItemState : IViewState {
        string QuestKey { get; }

        int Notifier { get; }

        bool IsSelected { get; }

        bool IsCompleted    { get; }
        bool CanBeCompleted { get; }

        bool IsRevealed    { get; }
        bool CanBeRevealed { get; }

        void Select();
    }
}