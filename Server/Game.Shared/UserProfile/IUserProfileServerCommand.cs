namespace Game.Shared.UserProfile {
    using Commands;
    using Commands.CoinFarm;
    using Commands.Currencies;
    using Commands.Exp;
    using Commands.Features;
    using Commands.Game;
    using Commands.GameModes;
    using Commands.Gunsmiths;
    using Commands.Loadouts;
    using Commands.Quests;
    using Commands.Rewards;
    using Commands.Rewards.Impl;
    using Commands.Thresher;
    using Commands.TraderShop;
    using Commands.Tutorials;
    using Commands.Storage;
    using Commands.FeatureToggles;
    using Commands.MailBox;
    using Data;
    using MessagePack;
    using Multicast;

    // чтобы при разработке индексы команд не пересекались
    // каждый программист дает индексы со своим префиксом
    //
    // VladV - префикс 0xx (1, 2, 3, ...)
    [Union(1, typeof(UserProfileActivateCommand))]
    // Gameplay
    [Union(2, typeof(UserProfileJoinGameCommand))]
    //[Union(3, typeof(UserProfileReportGameSnapshotCommand))]
    [Union(4, typeof(UserProfileApplyGameResultsCommand))]
    [Union(5, typeof(UserProfileConfirmGameResultCommand))]
    [Union(6, typeof(UserProfileFetchCommand))]
    [Union(7, typeof(UserProfileLeaveAllGamesCommand))]
    // Currency
    [Union(20, typeof(UserProfileAddCurrencyCommand))]
    [Union(21, typeof(UserProfileCurrencyTakeAmountCommand))]
    [Union(22, typeof(UserProfileDebugCheatAddCurrencyCommand))]
    // Rewards
    [Union(40, typeof(UserProfileQueueRewardCommand))]
    [Union(41, typeof(UserProfileOpenRewardCommand))]
    [Union(42, typeof(UserProfileOpenCurrencyRewardCommand))]
    // Loadout
    [Union(60, typeof(UserProfileModifyLoadoutCommand))]
    [Union(61, typeof(EnsureInitialLoadoutCommand))]
    [Union(62, typeof(UserProfileSelectLoadoutCommand))]
    // TraderShop
    [Union(80, typeof(UserProfileTraderShopMakeDealCommand))]
    [Union(81, typeof(UserProfileTraderShopRefreshCommand))]
    // GameModes
    [Union(90, typeof(UserProfileSelectGameModeCommand))]
    // Quests
    [Union(100, typeof(UserProfileApplyCounterQuestsCommand))]
    //[Union(101, typeof(UserProfileClaimAllQuestsCommand))]
    //[Union(102, typeof(UserProfileRevealNewQuestsCommand))]
    [Union(103, typeof(UserProfileClaimQuestCommand))]
    [Union(104, typeof(UserProfileRevealQuestCommand))]
    [Union(105, typeof(UserProfileDonateItemQuestsCommand))]
#if DEBUG
    [Union(106, typeof(UserProfileDebugCheatToggleQuestTaskCommand))]
#endif
    // Exp
    [Union(120, typeof(UserProfileLevelUpCommand))]
    // Thresher
    //[Union(130, typeof(UserProfileThresherThreshCommand))]
    [Union(131, typeof(UserProfileThresherLevelUpCommand))]
    //NickName
    [Union(141, typeof(UserProfileSetNickNameCommand))]
    // Gunsmith
    [Union(160, typeof(UserProfileGunsmithRefreshCommand))]
    [Union(161, typeof(UserProfileGunsmithBuyLoadoutCommand))]
    // Tutorial
    [Union(171, typeof(UserProfileCompleteTutorialCommand))]
    // Features
    [Union(181, typeof(UserProfileViewFeatureCommand))]
    //
    [Union(191, typeof(UserProfileSetFeatureTogglesCommand))]
    //
    [Union(200, typeof(UserProfileCoinFarmCollectCommand))]
    //
    [Union(210, typeof(UserProfileMailBoxClaimMessageCommand))]
    [Union(211, typeof(UserProfileMailBoxViewAllMessagesCommand))]
    //
    // EugeneT - префикс 1xxx (1001, 1002, 1003, ...)
    //
    // AlekseiiZ - префикс 2ххх (2001, 2002, 20003, ...)
    //
    [Union(2001, typeof(UserProfileQueueDropRewardCommand))]
    [Union(2002, typeof(PurchaseCurrencyCommand))]
    [Union(2003, typeof(UserProfileSyncStorageCommand))]
    public interface IUserProfileServerCommand : IServerCommand<SdUserProfile> {
    }
}