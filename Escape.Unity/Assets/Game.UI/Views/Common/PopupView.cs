namespace Game.UI.Views.Common {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class PopupView : AutoView<IPopupState> {
        [SerializeField, Required] private ViewPanel     contentPanel;
        [SerializeField]           private RectTransform contentRoot;

        private Vector2 initialContentSize;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("popup_key", () => this.State.PopupKey, "SettingsMenu"),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close_clocked", () => this.State.OnClose()),
        };

        protected override void Awake() {
            base.Awake();

            if (this.contentRoot) {
                this.initialContentSize = this.contentRoot.sizeDelta;
            }
        }

        protected override void Render() {
            base.Render();

            if (this.contentRoot) {
                this.contentRoot.sizeDelta = this.initialContentSize +
                                             Vector2.up * this.State.ContentHeight;
            }

            this.contentPanel.Render(this.State.Content);
        }
    }

    public interface IPopupState : IViewState {
        string PopupKey { get; }

        IState Content       { get; }
        float  ContentHeight { get; }

        void OnClose();
    }
}