namespace Game.UI.Views.Common {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using UnityEngine.UI;

    public class AlertDialogView : AutoView<IAlertDialogState> {
        [SerializeField, Required] private ViewPanel     buttonsPanels;
        [SerializeField, Required] private LayoutElement buttonsLayout;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("title_localization_key", () => this.State.TitleLocalizationKey, "ALERT_TITLE_QUIT_GAME"),
            this.Variable("message_localization_key", () => this.State.MessageLocalizationKey, "ALERT_MESSAGE_QUIT_GAME"),
            this.Variable("closeable", () => this.State.Closeable),
            this.Variable("arg1", () => this.State.GetArgument(0), "ARG1"),
            this.Variable("arg2", () => this.State.GetArgument(1), "ARG2"),
            this.Variable("arg3", () => this.State.GetArgument(2), "ARG3"),
            this.Variable("arg4", () => this.State.GetArgument(3), "ARG4"),
            this.Variable("arg5", () => this.State.GetArgument(4), "ARG5"),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };

        protected override void Render() {
            base.Render();

            this.buttonsPanels.Render(this.State.Buttons);
            this.buttonsLayout.preferredHeight = this.State.Buttons.Size.MinHeight;
        }
    }

    public interface IAlertDialogState : IViewState {
        string TitleLocalizationKey   { get; }
        string MessageLocalizationKey { get; }

        string GetArgument(int index);

        bool Closeable { get; }

        IState Buttons { get; }

        void Close();
    }
}