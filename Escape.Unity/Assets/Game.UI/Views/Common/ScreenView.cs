namespace Game.UI.Views.Common {
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class ScreenView : AutoView<IScreenState> {
        [SerializeField, Required] private ViewPanel contentPanel;
        [SerializeField, Required] private ViewPanel headerPanel;

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close_clocked", () => this.State.OnClose()),
        };

        protected override void Render() {
            base.Render();

            this.headerPanel.Render(this.State.Header);
            this.contentPanel.Render(this.State.Content);
        }
    }

    public interface IScreenState : IViewState {
        IState Content { get; }
        IState Header  { get; }

        void OnClose();
    }
}