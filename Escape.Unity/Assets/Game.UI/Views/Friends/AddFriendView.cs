namespace Game.UI.Views.Friends {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class AddFriendView : AutoView<IAddFriendViewState> {
        [SerializeField, Required] private TMP_InputField nicknameInputField;
        [SerializeField, Required] private Button         submitButton;
        [SerializeField, Required] private TMP_Text       statusText;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("nickname", () => this.State.Nickname, string.Empty),
            this.Variable("status", () => this.State.StatusMessage, string.Empty),
            this.Variable("canSubmit", () => this.State.CanSubmit, false),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };

        protected override void Awake() {
            base.Awake();

            this.submitButton.Click(() => {
                if (!this.HasState) {
                    return;
                }

                var nickname = this.nicknameInputField.text;
                this.State.TrySubmit(nickname);

                this.nicknameInputField.text = string.Empty;
                this.nicknameInputField.Select();
                this.nicknameInputField.ActivateInputField();
            });

            nicknameInputField.onValueChanged.AddListener(nick => {
                if (this.HasState) {
                    this.State.OnNicknameChanged(nick);
                }
            });
        }

        protected override void Activate() {
            base.Activate();

            this.nicknameInputField.text = this.State.Nickname;
            this.nicknameInputField.Select();
            this.nicknameInputField.ActivateInputField();
        }

        protected override void Deactivate() {
            base.Deactivate();

            this.nicknameInputField.DeactivateInputField();
        }

        protected override void Render() {
            base.Render();

            this.submitButton.interactable = this.State.CanSubmit;

            if (this.statusText != null) {
                this.statusText.text = this.State.StatusMessage ?? string.Empty;
            }
        }
    }

    public interface IAddFriendViewState : IViewState {
        string Nickname      { get; }
        string StatusMessage { get; }
        bool   CanSubmit     { get; }

        void TrySubmit(string nickname);
        void OnNicknameChanged(string value);
        void Close();
    }
}