namespace Game.UI.Views.MainMenu {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class MainMenuView : AutoView<IMainMenuState> {
        [SerializeField, Required] private MainMenuLevelView                 levelView;
        [SerializeField, Required] private MainMenuExpProgressionRewardsView expProgressionRewardsView;
        [SerializeField, Required] private MainMenuCoinFarmView              badgesCoinFarmView;

        [SerializeField, Required] private MainMenuButtonView mailBoxButton;

        [SerializeField, Required] private MainMenuButtonView storageButton;
        [SerializeField, Required] private MainMenuButtonView traderShopButton;
        [SerializeField, Required] private MainMenuButtonView questsButton;
        [SerializeField, Required] private MainMenuButtonView friendsButton;
        [SerializeField, Required] private MainMenuButtonView threshersButton;
        [SerializeField, Required] private MainMenuButtonView gunsmithButton;
        [SerializeField, Required] private MainMenuButtonView blackMarketButton;
        [SerializeField, Required] private MainMenuButtonView settingsButton;

        [SerializeField, Required] private PartyStatusView        partyStatusView;
        [SerializeField, Required] private MainMenuPlayButtonView playButtonView;

        [SerializeField, Required] private ViewPanel worldPanel;

        protected override void Render() {
            base.Render();

            this.levelView.Render(this.State.Level);
            this.expProgressionRewardsView.Render(this.State.ExpProgressionRewards);
            this.badgesCoinFarmView.Render(this.State.BadgesCoinFarm);
            
            this.mailBoxButton.Render(this.State.MailBoxButton);

            this.storageButton.Render(this.State.StorageButton);
            this.traderShopButton.Render(this.State.TraderShopButton);
            this.questsButton.Render(this.State.QuestsButton);
            this.friendsButton.Render(this.State.FriendsButton);
            this.threshersButton.Render(this.State.ThreshersButton);
            this.gunsmithButton.Render(this.State.GunsmithButton);
            this.blackMarketButton.Render(this.State.BlackMarketButton);
            this.settingsButton.Render(this.State.SettingsButton);
            
            this.partyStatusView.Render(this.State.PartyStatusState);
            this.playButtonView.Render(this.State.PlayButton);
            
            this.worldPanel.Render(this.State.WorldViews);
        }
    }

    public interface IMainMenuState : IViewState {
        IMainMenuLevelState                 Level                 { get; }
        IMainMenuExpProgressionRewardsState ExpProgressionRewards { get; }
        IMainMenuCoinFarmState              BadgesCoinFarm        { get; }

        IMainMenuButtonState MailBoxButton { get; }

        IMainMenuButtonState StorageButton     { get; }
        IMainMenuButtonState ThreshersButton   { get; }
        IMainMenuButtonState TraderShopButton  { get; }
        IMainMenuButtonState QuestsButton      { get; }
        IMainMenuButtonState FriendsButton     { get; }
        IMainMenuButtonState GunsmithButton    { get; }
        IMainMenuButtonState BlackMarketButton { get; }
        IMainMenuButtonState SettingsButton    { get; }

        IPartyStatusState        PartyStatusState { get; }
        IMainMenuPlayButtonState PlayButton       { get; }

        IState WorldViews { get; }
    }
}