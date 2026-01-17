namespace Game.UI.Widgets.MainMenu {
    using Controllers.Features.NameEdit;
    using Domain.Currencies;
    using Multicast;
    using Shared;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using UniMob;
    using UniMob.UI;
    using Views.MainMenu;

    [RequireFieldsInit]
    public class MainMenuLevelWidget : StatefulWidget {
    }

    public class MainMenuLevelState : ViewState<MainMenuLevelWidget>, IMainMenuLevelState {
        [Inject] private SdUserProfile   userProfile;
        [Inject] private CurrenciesModel currenciesModel;
        [Inject] private GameDef         gameDef;

        [Atom] private LevelDef LevelDef => this.gameDef.GetLevel(this.userProfile.Level.Value);

        public override WidgetViewReference View => default;


        public string NickName => this.userProfile.NickName.Value;

        public int Level => this.userProfile.Level.Value;

        public int CurrentExp => this.userProfile.Exp.Value;

        public int TotalExp => this.LevelDef.expToNextLevel;

        public int Rating => this.currenciesModel.Rating.Amount;

        public void EditNickName() {
            NameEditFeatureEvents.Prompt.Raise();
        }
    }
}