namespace Game.Shared.UserProfile.Data {
    using CoinFarms;
    using Currencies;
    using DTO;
    using ExpProgressionRewards;
    using Features;
    using Gunsmiths;
    using Items;
    using Loadouts;
    using MailBox;
    using Multicast.FeatureToggles;
    using Multicast.ServerData;
    using Quests;
    using Rewards;
    using Storage;
    using Store;
    using Threshers;
    using Tutorials;

    public class SdUserProfile : SdObject {
        public static SdUserProfile Create(ISdObjectTracker tracker = null) => SdFactory.Create(args => new SdUserProfile(args), tracker);

        public SdValue<string> NickName      { get; }
        public SdValue<int>    Level         { get; }
        public SdValue<int>    Exp           { get; }
        public SdValue<int>    StorageWidth  { get; }
        public SdValue<int>    StorageHeight { get; }
        public SdMailBox       MailBox       { get; }

        public SdFeatureToggleRepo            FeatureToggles        { get; }
        public SdRepo<SdFeature>              Features              { get; }
        public SdRewardRepo                   Rewards               { get; }
        public SdCurrencyRepo                 Currencies            { get; }
        public SdRepo<SdItem>                 Items                 { get; }
        public SdGameModesRepo                GameModes             { get; }
        public SdLoadoutsRepo                 Loadouts              { get; }
        public SdStorageItemRepo              Storage               { get; }
        public SdRepo<SdQuest>                Quests                { get; }
        public SdRepo<SdQuestCounterTask>     QuestCounterTasks     { get; }
        public SdRepo<SdQuestDonateItemTask>  QuestDonateItemTasks  { get; }
        public SdRepo<SdThresher>             Threshers             { get; }
        public SdRepo<SdGunsmith>             Gunsmiths             { get; }
        public SdRepo<SdExpProgressionReward> ExpProgressionRewards { get; }
        public SdTutorialRepo                 Tutorials             { get; }

        public SdDict<SdGameResult> PlayedGames { get; }

        public SdValue<TraderShopState> TraderShop { get; }

        public SdRepo<SdStoreItem>        StoreItems        { get; }
        public SdRepo<SdCurrencyPurchase> CurrencyPurchases { get; }
        public SdRepo<SdCoinFarm>         CoinFarms         { get; }

        public SdUserProfile(SdArgs args) : base(args) {
            this.NickName              = this.Child("nick");
            this.Level                 = new SdValue<int>(this.Child("lvl"), 1);
            this.Exp                   = this.Child("exp");
            this.StorageWidth          = new SdValue<int>(this.Child("storage_w"), 6);
            this.StorageHeight         = new SdValue<int>(this.Child("storage_h"), 8);
            this.MailBox               = new SdMailBox(this.Child("mailbox"));
            this.FeatureToggles        = new SdFeatureToggleRepo(this.Child("feature_toggles"));
            this.Features              = new SdRepo<SdFeature>(this.Child("features"), a => new SdFeature(a));
            this.Rewards               = new SdRewardRepo(this.Child("rewards"));
            this.Currencies            = new SdCurrencyRepo(this.Child("currencies"), a => new SdCurrency(a));
            this.Items                 = new SdRepo<SdItem>(this.Child("items"), a => new SdItem(a));
            this.GameModes             = new SdGameModesRepo(this.Child("game_modes"), a => new SdGameMode(a));
            this.Loadouts              = new SdLoadoutsRepo(this.Child("loadouts"), a => new SdLoadout(a));
            this.Storage               = new SdStorageItemRepo(this.Child("storage"), a => new SdStorageItem(a));
            this.Quests                = new SdRepo<SdQuest>(this.Child("quests"), a => new SdQuest(a));
            this.QuestCounterTasks     = new SdRepo<SdQuestCounterTask>(this.Child("quests_c"), a => new SdQuestCounterTask(a));
            this.QuestDonateItemTasks  = new SdRepo<SdQuestDonateItemTask>(this.Child("quests_d"), a => new SdQuestDonateItemTask(a));
            this.PlayedGames           = new SdDict<SdGameResult>(this.Child("played_games"), a => new SdGameResult(a));
            this.Threshers             = new SdRepo<SdThresher>(this.Child("threshers"), a => new SdThresher(a));
            this.Gunsmiths             = new SdRepo<SdGunsmith>(this.Child("gunsmiths"), a => new SdGunsmith(a));
            this.ExpProgressionRewards = new SdRepo<SdExpProgressionReward>(this.Child("exp_p_rewards"), a => new SdExpProgressionReward(a));
            this.Tutorials             = new SdTutorialRepo(this.Child("tutorials"));
            this.TraderShop            = this.Child("trader_shop_item_v1");
            this.StoreItems            = new SdRepo<SdStoreItem>(this.Child("store_items_v1"), a => new SdStoreItem(a));
            this.CurrencyPurchases     = new SdRepo<SdCurrencyPurchase>(this.Child("currency_purchases"), a => new SdCurrencyPurchase(a));
            this.CoinFarms             = new SdRepo<SdCoinFarm>(this.Child("coin_farms"), a => new SdCoinFarm(a));
        }
    }
}