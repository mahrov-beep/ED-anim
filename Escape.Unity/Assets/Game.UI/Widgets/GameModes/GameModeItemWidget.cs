namespace Game.UI.Widgets.GameModes {
    using System;
    using Domain.GameModes;
    using Multicast;
    using Shared.UserProfile.Data;
    using UniMob;
    using UniMob.UI;
    using Views.GameModes;

    [RequireFieldsInit]
    public class GameModeItemWidget : StatefulWidget {
        public string GameModeKey;

        public bool IsSelected;

        public Action OnSelect;
    }

    public class GameModeItemState : ViewState<GameModeItemWidget>, IGameModeItemState {
        [Inject] private GameModesModel gameModesModel;
        [Inject] private SdUserProfile  userProfile;

        [Atom] private GameModeModel GameModeModel => this.gameModesModel.Get(this.Widget.GameModeKey);

        public override WidgetViewReference View => UiConstants.Views.GameModes.Item;

        public string GameModeKey => this.GameModeModel.Key;
        public bool   IsSelected  => this.Widget.IsSelected;

        public int RequiredProfileLevel => this.GameModeModel.MinProfileLevel;
        public int CurrentProfileLevel  => this.userProfile.Level.Value;

        public void Select() {
            this.Widget.OnSelect?.Invoke();
        }
    }
}