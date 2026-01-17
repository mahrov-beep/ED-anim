namespace Game.UI.Views.Threshers {
    using UniMob.UI;
    using Multicast;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class ThreshersMenuView : AutoView<IThreshersMenuState> {
        [SerializeField, Required] private ViewPanel threshersList;
        [SerializeField, Required] private ViewPanel thresher;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("selected_thresher_key", () => this.State.SelectedThresherKey, SharedConstants.Game.Threshers.TRADER),
            this.Variable("can_level_up", () => this.State.CanLevelUp, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
            this.Event("level_up", () => this.State.LevelUp()),
        };

        protected override void Render() {
            base.Render();

            this.threshersList.Render(this.State.ThreshersList);
            this.thresher.Render(this.State.Thresher);
        }
    }

    public interface IThreshersMenuState : IViewState {
        string SelectedThresherKey { get; }

        IState ThreshersList { get; }
        IState Thresher      { get; }

        bool CanLevelUp { get; }

        void Close();
        void LevelUp();
    }
}