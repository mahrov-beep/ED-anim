namespace Game.UI.Modules {
    using Controllers.Features.Storage;
    using Controllers.Tutorial;
    using Controllers.Tutorial.Sequences;
    using Cysharp.Threading.Tasks;
    using Domain.Currencies;
    using Domain.Game;
    using Domain.GameInventory;
    using Domain.GameModes;
    using Domain;
    using Domain.CoinFarms;
    using Domain.ExpProgressionRewards;
    using Domain.Features;
    using Domain.Gunsmiths;
    using Domain.ItemBoxStorage;
    using Domain.items;
    using Domain.MailBox;
    using Domain.Party;
    using Domain.Models.Purchases;
    using Domain.Safe;
    using Domain.Quests;
    using Domain.Storage;
    using Domain.Threshers;
    using Domain.TraderShop;
    using Domain.Settings;
    using ECS.Systems.Input;
    using ECS.Systems.Unit;
    using ECS.Systems.Grenade;
    using Game.ECS.Scripts;
    using Game.Services.Cheats;
    using Multicast;
    using Multicast.Install;
    using Multicast.Server;
    using Services.Photon;
    using Services.Graphics;
    using Shared.Defs;
    using Shared.UserProfile;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.CoinFarms;
    using Shared.UserProfile.Data.Currencies;
    using Shared.UserProfile.Data.ExpProgressionRewards;
    using Shared.UserProfile.Data.Gunsmiths;
    using Shared.UserProfile.Data.Items;
    using Shared.UserProfile.Data.Quests;
    using Shared.UserProfile.Data.Store;
    using Shared.UserProfile.Data.Threshers;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Widgets.Game;
    using Widgets.GameInventory;
    using Widgets.Storage;
    using Widgets.TraderShop;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_FEATURE)]
    public class GameMainModule : ScriptableModule, ISubModuleProvider {
        [SerializeField] private PlayerInputConfig      playerInputConfig;
        [SerializeField] private FloatingTextConfig     floatingTextConfig;
        [SerializeField] private CueUIVisualizeConfig   cueUIVisualizeConfig;
        [SerializeField] private MinimapConfig          minimapConfig;
        [SerializeField] private GrenadeIndicatorConfig grenadeIndicatorConfig;
        [SerializeField] private ItemBoxOutlineConfig   itemBoxOutlineConfig;
        [SerializeField] private GraphicsSettingsConfig graphicsSettingsConfig;
        [SerializeField] private InputActionAsset       inputActionAsset;

        public override void Setup(ModuleSetup module) {
        }

        public override UniTask Install(Resolver resolver) => UniTask.CompletedTask;

        public IScriptableModule[] BuildSubModules() => new[] {
            ScriptableModuleFactory.Service(
                new UserProfileServerCommandContext(
                    App.CreateServerClientSideExecutor<UserProfileServerCommandContext, SdUserProfile, IUserProfileServerCommand>()
                )
            ),
            ScriptableModuleFactory.Service(SdUserProfile.Create(new SdObjectAtomTracker())),
            ScriptableModuleFactory.Service<PhotonService, PhotonService>(),
            ScriptableModuleFactory.Service<GameInventoryApi, GameInventoryApi>(),
            ScriptableModuleFactory.Service<GameNearbyItemsApi, GameNearbyItemsApi>(),
            ScriptableModuleFactory.Service<StorageApi, StorageApi>(),
            ScriptableModuleFactory.Service<TraderShopApi, TraderShopApi>(),
            ScriptableModuleFactory.Service<IGraphicsSettingStorage, PlayerPrefsGraphicsSettingStorage>(),
            ScriptableModuleFactory.Service<IGraphicsSettingsModel, GraphicsSettingsModel>(),
            ScriptableModuleFactory.Service<GraphicsSettingsService, GraphicsSettingsService>(),
//            ScriptableModuleFactory.Service<LightmapPresetService, LightmapPresetService>(),
            ScriptableModuleFactory.Service<IDebugFireSettingsService, DebugFireSettingsService>(),

            // Tutorial Sequences
            ScriptableModuleFactory.Service<TutorialService, TutorialService>(),
            ScriptableModuleFactory.Service<FirstPlayTutorialSequence, FirstPlayTutorialSequence>(),
            ScriptableModuleFactory.Service<GunsmithBuyLoadoutTutorialSequence, GunsmithBuyLoadoutTutorialSequence>(),

            // Assets
            // ReSharper disable RedundantTypeArgumentsOfMethod
            ScriptableModuleFactory.Asset<PlayerInputConfig>(playerInputConfig),
            ScriptableModuleFactory.Asset<FloatingTextConfig>(floatingTextConfig),
            ScriptableModuleFactory.Asset<CueUIVisualizeConfig>(cueUIVisualizeConfig),
            ScriptableModuleFactory.Asset<MinimapConfig>(minimapConfig),
            ScriptableModuleFactory.Asset<GrenadeIndicatorConfig>(grenadeIndicatorConfig),
            ScriptableModuleFactory.Asset<ItemBoxOutlineConfig>(itemBoxOutlineConfig),
            ScriptableModuleFactory.Asset<GraphicsSettingsConfig>(graphicsSettingsConfig),
            ScriptableModuleFactory.Asset<InputActionAsset>(inputActionAsset),
            // ReSharper restore RedundantTypeArgumentsOfMethod

            // Models
            ScriptableModuleFactory.Model<SystemModel>(),
            ScriptableModuleFactory.Model<GameLocalCharacterModel>(),
            ScriptableModuleFactory.Model<GameStateModel>(),
            ScriptableModuleFactory.Model<GameInventoryModel>(),
            ScriptableModuleFactory.Model<GameNearbyItemsModel>(),
            ScriptableModuleFactory.Model<GameNearbyInteractiveZoneModel>(),
            ScriptableModuleFactory.Model<TraderShopModel>(),
            ScriptableModuleFactory.Model<MapModel>(),
            ScriptableModuleFactory.Model<ListenedCueModel>(),
            ScriptableModuleFactory.Model<GrenadeIndicatorModel>(),
            ScriptableModuleFactory.Model<DamageSourceModel>(),
            ScriptableModuleFactory.Model<SafeModel>(),
            ScriptableModuleFactory.Model<StorageModel>(),
            ScriptableModuleFactory.Model<ItemBoxStorageModel>(),
            ScriptableModuleFactory.Model<FeaturesModel>(),
            ScriptableModuleFactory.Model<MailBoxModel>(),
            ScriptableModuleFactory.Model<PartyModel>(),

            //KeyedModels
            ScriptableModuleFactory.KeyedModel<CurrencyDef, SdCurrency, CurrencyModel, CurrenciesModel>(),
            ScriptableModuleFactory.KeyedModel<StoreItemDef, SdStoreItem, StoreItemModel, StoreItemsModel>(),
            ScriptableModuleFactory.KeyedModel<CurrencyPurchaseDef, SdCurrencyPurchase, CurrencyPurchaseModel, CurrencyPurchasesModel>(),
            ScriptableModuleFactory.KeyedModel<GameModeDef, SdGameMode, GameModeModel, GameModesModel>(),
            ScriptableModuleFactory.KeyedModel<ThresherDef, SdThresher, ThresherModel, ThreshersModel>(),
            ScriptableModuleFactory.KeyedModel<GunsmithDef, SdGunsmith, GunsmithModel, GunsmithsModel>(),
            ScriptableModuleFactory.KeyedModel<ItemDef, SdItem, ItemModel, ItemsModel>(),
            ScriptableModuleFactory.KeyedModel<QuestDef, SdQuest, QuestModel, QuestsModel>(),
            ScriptableModuleFactory.KeyedModel<QuestCounterTaskDef, SdQuestCounterTask, QuestCounterTaskModel, QuestCounterTasksModel>(),
            ScriptableModuleFactory.KeyedModel<QuestDonateItemTaskDef, SdQuestDonateItemTask, QuestDonateItemTaskModel, QuestDonateItemTasksModel>(),
            ScriptableModuleFactory.KeyedModel<ExpProgressionRewardDef, SdExpProgressionReward, ExpProgressionRewardModel, ExpProgressionRewardsModel>(),
            ScriptableModuleFactory.KeyedModel<CoinFarmDef, SdCoinFarm, CoinFarmModel, CoinFarmsModel>(),
        };
    }
}
