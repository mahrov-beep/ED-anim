namespace Game.UI.Widgets.Friends {
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UnityEngine;
    using Views.Friends;

    [RequireFieldsInit]
    public class AddFriendWidget : StatefulWidget {
        public System.Action<string> OnSubmit;
        public System.Action         OnClose;
    }

    public class AddFriendState : ViewState<AddFriendWidget>, IAddFriendViewState {
        [Atom] public string Nickname      { get; private set; } = string.Empty;
        [Atom] public string StatusMessage { get; private set; } = string.Empty;

        public override WidgetViewReference View => UiConstants.Views.Friends.AddFriend;

        [Atom]
        public bool CanSubmit => !string.IsNullOrWhiteSpace(this.Nickname);

        public void TrySubmit(string name) {
            this.Widget.OnSubmit?.Invoke(name);
        }

        public void OnNicknameChanged(string value) {
            this.Nickname      = value;
            this.StatusMessage = string.Empty;
        }

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }
    }
}