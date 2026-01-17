namespace Game.UI.Widgets.EditName {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Shared.UserProfile.Data;
    using UniMob;
    using UniMob.UI;
    using Views.EditName;

    [RequireFieldsInit]
    public class EditNameDialogWidget : StatefulWidget {
        public Action<(bool confirmed, string newNickName)> OnConfirm;
        public Func<string, UniTask<bool>>                  RequestChangeNickName;
    }

    public class EditNameDialogState : ViewState<EditNameDialogWidget>, IEditNameDialogState {
        [Inject] private SdUserProfile userProfile;

        [Atom] public bool   WaitServerResponse  { get; private set; }
        [Atom] public string ServerStatusMessage { get; private set; }

        public override WidgetViewReference View => UiConstants.Views.EditName.Dialog;

        public string CurrentNickName => this.userProfile.NickName.Value;

        public void TryConfirm(string newNickName) {
            ProcessConfirm(newNickName).Forget();
        }

        public void Cancel() {
            this.Widget.OnConfirm?.Invoke((false, null));
        }

        private async UniTask ProcessConfirm(string newNickName) {
            if (this.WaitServerResponse) {
                return;
            }

            this.WaitServerResponse  = true;
            this.ServerStatusMessage = string.Empty;

            var isAvailable = false;

            if (this.Widget.RequestChangeNickName != null) {
                this.ServerStatusMessage = "Waiting server response.";

                isAvailable = await this.Widget.RequestChangeNickName(newNickName);
            }

            if (isAvailable) {
                this.Widget.OnConfirm?.Invoke((true, newNickName));
            }
            else {
                this.ServerStatusMessage      = "Name already taken.";
            }

            this.WaitServerResponse = false;
        }
    }
}