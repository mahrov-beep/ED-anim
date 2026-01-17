namespace Game.UI.Widgets.MainMenu {
    using System;
    using Controllers.Features.ExpProgressionRewards;
    using Controllers.Features.Friends;
    using Controllers.Features.Gunsmith;
    using Controllers.Features.MailBox;
    using Controllers.Features.Quest;
    using Controllers.Features.Storage;
    using Controllers.Features.Thresher;
    using Controllers.Features.TraderShop;
    using Controllers.Features.Settings;
    using Domain.ExpProgressionRewards;
    using Domain.Features;
    using Domain.MailBox;
    using Domain.Quests;
    using Domain.Threshers;
    using Multicast;
    using Party;
    using Shared;
    using UniMob;
    using UniMob.UI;
    using Views.MainMenu;

    public class MainMenuWidget : StatefulWidget {
        public int FriendRequestCount { get; set; }

        public string[] OnlineIds { get; set; }
        public string[] InGameIds { get; set; }
    }

    public class MainMenuState : ViewState<MainMenuWidget>, IMainMenuState {
        [Inject] private ThreshersModel             threshersModel;
        [Inject] private FeaturesModel              featuresModel;
        [Inject] private QuestsModel                questsModel;
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;
        [Inject] private MailBoxModel               mailBoxModel;

        public override WidgetViewReference View => UiConstants.Views.MainMenu.Screen;
        
        [Atom] public IMainMenuLevelState Level =>
            this.RenderChildT(_ => new MainMenuLevelWidget()).As<MainMenuLevelState>();

        [Atom] public IMainMenuExpProgressionRewardsState ExpProgressionRewards =>
            this.RenderChildT(_ => new MainMenuExpProgressionRewardsWidget()).As<MainMenuExpProgressionRewardsState>();

        [Atom] public IMainMenuCoinFarmState BadgesCoinFarm => this.RenderChildT(_ => new MainMenuCoinFarmWidget {
            CoinFarmKey = SharedConstants.Game.CoinFarms.COIN_FARM_BADGES,
        }).As<MainMenuCoinFarmState>();

        [Atom] public IMainMenuButtonState MailBoxButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter = this.mailBoxModel.Notify,
            OnClick         = () => MailBoxFeatureEvents.Open.Raise(),
        }).As<MainMenuButtonState>();

        [Atom] public IMainMenuButtonState StorageButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter = 0,
            OnClick         = () => StorageFeatureEvents.Open.Raise(),
        }).As<MainMenuButtonState>();

        [Atom] public IMainMenuButtonState ThreshersButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter = this.threshersModel.Notify,
            OnClick         = () => ThresherFeatureEvents.Open.Raise(),
        }).As<MainMenuButtonState>();

        [Atom] public IMainMenuButtonState TraderShopButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter    = 0,
            LockedByFeatureKey = SharedConstants.Game.Features.TRADER_SHOP,
            OnClick = () => this.CallOrShowFeatureLock(SharedConstants.Game.Features.TRADER_SHOP,
                () => TraderShopFeatureEvents.Open.Raise()
            ),
        }).As<MainMenuButtonState>();

        [Atom] public IMainMenuButtonState QuestsButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter = this.questsModel.Notify,
            OnClick         = () => QuestFeatureEvents.Open.Raise(),
        }).As<MainMenuButtonState>();

        [Atom] public IMainMenuButtonState FriendsButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter = this.Widget.FriendRequestCount,
            OnClick         = () => FriendsFeatureEvents.Open.Raise(),
        }).As<MainMenuButtonState>();

        [Atom] public IMainMenuButtonState GunsmithButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter    = 0,
            LockedByFeatureKey = SharedConstants.Game.Features.GUNSMITH,
            OnClick = () => this.CallOrShowFeatureLock(SharedConstants.Game.Features.GUNSMITH,
                () => GunsmithFeatureEvents.Open.Raise()
            ),
        }).As<MainMenuButtonState>();

        [Atom] public IMainMenuButtonState BlackMarketButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter    = 0,
            LockedByFeatureKey = SharedConstants.Game.Features.BLACK_MARKET,
            OnClick = () => this.CallOrShowFeatureLock(SharedConstants.Game.Features.BLACK_MARKET,
                () => { }
            ),
        }).As<MainMenuButtonState>();

        [Atom] public IMainMenuButtonState SettingsButton => this.RenderChildT(_ => new MainMenuButtonWidget {
            NotifierCounter = 0,
            OnClick         = () => SettingsFeatureEvents.Open.Raise(),
        }).As<MainMenuButtonState>();

        [Atom] public IPartyStatusState PartyStatusState => this.RenderChildT(_ => new PartyStatusWidget() {
                InGameIds = this.Widget.InGameIds,
                OnlineIds = this.Widget.OnlineIds,
            }).As<PartyStatusState>();
        
        [Atom] public IState WorldViews => this.RenderChild(_ => new MainMenuDynamicWidget());

        [Atom] public IMainMenuPlayButtonState PlayButton => this.RenderChildT(_ => new MainMenuPlayButtonWidget())
            .As<MainMenuPlayButtonState>();

        private void CallOrShowFeatureLock(string featureKey, Action call) {
            if (this.featuresModel.IsFeatureUnlocked(featureKey)) {
                call();
                return;
            }
 
            if (this.featuresModel.TryGetFeatureUnlockExpProgressionReward(featureKey, out var model)) {
                this.expProgressionRewardsModel.Selected = model;
                ExpProgressionRewardsFeatureEvents.Open.Raise();
            }
        }
    }
}