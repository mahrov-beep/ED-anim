namespace Game.UI.Views.EditName {
    using UniMob.UI;
    using Multicast;
    using Shared.UserProfile.Commands;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class EditNameDialogView : AutoView<IEditNameDialogState> {
        [SerializeField, Required] private TMP_InputField nameInputField;
        [SerializeField, Required] private Button         confirmButton;
        [SerializeField, Required] private Button         cancelButton;
        [SerializeField, Required] private TMP_Text       serverStatusText;

        protected override void Awake() {
            base.Awake();

            this.cancelButton.Click(() => {
                if (this.HasState) {
                    this.State.Cancel();
                }
            });

            this.confirmButton.Click(() => {
                if (this.HasState) {
                    var newNickName = this.nameInputField.text;
                    this.State.TryConfirm(newNickName);
                }
            });

            this.nameInputField.onValidateInput = ValidateName;
        }

        protected override void Activate() {
            base.Activate();

            this.nameInputField.text = this.State.CurrentNickName;
            serverStatusText.text    = State.ServerStatusMessage;

            this.nameInputField.Select();
            this.nameInputField.ActivateInputField();
        }

        protected override void Deactivate() {
            base.Deactivate();

            this.nameInputField.DeactivateInputField();
        }

        protected override void Render() {
            base.Render();

            var notBusy = !State.WaitServerResponse;

            nameInputField.interactable = notBusy;
            confirmButton.interactable  = notBusy && this.nameInputField.text.Length > 0;

            serverStatusText.text = State.ServerStatusMessage;

            serverStatusText.color = State.WaitServerResponse
                            ? Color.yellow
                            : (string.IsNullOrEmpty(State.ServerStatusMessage) ?
                                            Color.white : Color.red);
        }

        private char ValidateName(string text, int pos, char ch) {
            if (text.Length >= UserProfileSetNickNameCommand.MaxLength ||
                !UserProfileSetNickNameCommand.IsValidChar(ch)) {
                return '\0';
            }

            return ch;
        }
    }

    public interface IEditNameDialogState : IViewState {
        string CurrentNickName { get; }

        bool   WaitServerResponse       { get; } 
        string ServerStatusMessage { get; } 

        void TryConfirm(string newNickName);
        void Cancel();
    }
}