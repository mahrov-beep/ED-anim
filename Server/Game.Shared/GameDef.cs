namespace Game.Shared {
    using Defs;
    using Multicast;
    using Multicast.Collections;
    using Multicast.DirtyDataEditor;
    using Multicast.FeatureToggles;
    using Multicast.Purchasing;
    using SoundEffects;

    public class GameDef {
        // Core
        public LookupCollection<FeatureToggleDef> FeatureToggles;

        // Game
        public LookupCollection<FeatureDef>              Features;
        public LookupCollection<CurrencyDef>             Currencies;
        public LookupCollection<LevelDef>                Levels;
        public LookupCollection<GameModeDef>             GameModes;
        public LookupCollection<ItemDef>                 Items;
        public LookupCollection<ItemSetupDef>            ItemSetups;
        public LookupCollection<QuestDef>                Quests;
        public LookupCollection<QuestCounterTaskDef>     QuestCounterTasks;
        public LookupCollection<QuestDonateItemTaskDef>  QuestDonateItemTasks;
        public LookupCollection<ThresherDef>             Threshers;
        public LookupCollection<GunsmithDef>             Gunsmiths;
        public LookupCollection<GunsmithLoadoutDef>      GunsmithLoadouts;
        public LookupCollection<PlayerLoadoutDef>        PlayerLoadouts;
        public LookupCollection<CurrencyPurchaseDef>     CurrencyPurchases;
        public LookupCollection<StoreItemDef>            StoreItems;
        public LookupCollection<StoreCategoryDef>        StoreCategories;
        public LookupCollection<PurchaseDef>             Purchases;
        public LookupCollection<SoundEffectDef>          SoundEffects;
        public LookupCollection<ExpProgressionRewardDef> ExpProgressionRewards;
        public LookupCollection<TutorialDef>             Tutorials;
        public LookupCollection<CoinFarmDef>             CoinFarms;

        public static GameDef FromCache(IEnumerableCache<TextAsset> cache) => new GameDef {
            // Core
            FeatureToggles = cache.GetLookup<FeatureToggleDef>(SharedConstants.Configs.FEATURE_TOGGLES),

            // Game
            Features              = cache.GetLookup<FeatureDef>(SharedConstants.Configs.FEATURES),
            Currencies            = cache.GetLookup<CurrencyDef>(SharedConstants.Configs.CURRENCIES),
            Levels                = cache.GetLookup<LevelDef>(SharedConstants.Configs.LEVELS),
            GameModes             = cache.GetLookup<GameModeDef>(SharedConstants.Configs.GAME_MODES),
            Items                 = cache.GetLookup<ItemDef>(SharedConstants.Configs.ITEMS),
            ItemSetups            = cache.GetLookup<ItemSetupDef>(SharedConstants.Configs.ITEM_SETUPS),
            Quests                = cache.GetLookup<QuestDef>(SharedConstants.Configs.QUESTS),
            QuestCounterTasks     = cache.GetLookup<QuestCounterTaskDef>(SharedConstants.Configs.QUEST_COUNTER_TASKS),
            QuestDonateItemTasks  = cache.GetLookup<QuestDonateItemTaskDef>(SharedConstants.Configs.QUEST_DONATE_ITEM_TASKS),
            Threshers             = cache.GetLookup<ThresherDef>(SharedConstants.Configs.THRESHERS),
            Gunsmiths             = cache.GetLookup<GunsmithDef>(SharedConstants.Configs.GUNSMITHS),
            GunsmithLoadouts      = cache.GetLookup<GunsmithLoadoutDef>(SharedConstants.Configs.GUNSMITH_LOADOUTS),
            PlayerLoadouts        = cache.GetLookup<PlayerLoadoutDef>(SharedConstants.Configs.PLAYER_LOADOUTS),
            CurrencyPurchases     = cache.GetLookup<CurrencyPurchaseDef>(SharedConstants.Configs.CURRENCY_PURCHASES),
            StoreItems            = cache.GetLookup<StoreItemDef>(SharedConstants.Configs.STORE_ITEMS),
            StoreCategories       = cache.GetLookup<StoreCategoryDef>(SharedConstants.Configs.STORE_CATEGORIES),
            Purchases             = cache.GetLookup<PurchaseDef>(SharedConstants.Configs.PURCHASES),
            SoundEffects          = cache.GetLookup<SoundEffectDef>(SharedConstants.Configs.SOUND_EFFECTS),
            ExpProgressionRewards = cache.GetLookup<ExpProgressionRewardDef>(SharedConstants.Configs.EXP_PROGRESSION_REWARDS),
            Tutorials             = cache.GetLookup<TutorialDef>(SharedConstants.Configs.TUTORIALS),
            CoinFarms             = cache.GetLookup<CoinFarmDef>(SharedConstants.Configs.COIN_FARMS),
        };

        public LevelDef GetLevel(int level) {
            var maxLevelDef = this.Levels.Items[0];

            foreach (var levelDef in this.Levels.Items) {
                if (level <= levelDef.level) {
                    return levelDef;
                }

                if (levelDef.level > maxLevelDef.level) {
                    maxLevelDef = levelDef;
                }
            }

            return maxLevelDef;
        }
    }
}