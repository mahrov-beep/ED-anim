namespace Game.Shared.UserProfile.Commands {
    using System.Linq;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Data.MailBox;
    using Defs;
    using GameModes;
    using Loadouts;
    using MailBox;
    using Multicast.Numerics;
    using Multicast.ServerData;
    using Quests;
    using Rewards;
    using Rewards.Impl;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileActivateCommand : IUserProfileServerCommand {
    }

    public class UserProfileActivateCommandHandler : UserProfileServerCommandHandler<UserProfileActivateCommand> {
        private readonly GameDef gameDef;

        public UserProfileActivateCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileActivateCommand command) {
            if (string.IsNullOrEmpty(gameData.GameModes.SelectedGameMode.Value)) {
                var defaultGameMode = this.gameDef.GameModes.Items.First(it => it.visible);

                await context.Execute(new UserProfileSelectGameModeCommand {
                    GameModeKey = defaultGameMode.key,
                });
            }

            // временный хак, чтобы отследить первую установку игры
            if (gameData.GameModes.Lookup.Count == 0) {
                await context.Execute(new UserProfileMailBoxQueueRewardMessageCommand {
                    Type = SdMailBoxMessageTypes.BetaTestReward,
                    Reward = RewardBuildUtility.Combine("beta_test_reward",
                        RewardBuildUtility.BuildCurrency(SharedConstants.Game.Currencies.BADGES, 1000),
                        RewardBuildUtility.BuildCurrency(SharedConstants.Game.Currencies.BUCKS, 100),
                        RewardBuildUtility.BuildItem(SharedConstants.Game.Items.WEAPON_AR),
                        RewardBuildUtility.BuildItem(SharedConstants.Game.Items.ATTACHMENT_SCOPE_HOLO)
                    ),
                });
            }

            gameData.Features.Lookup.ConfigureDataFrom(this.gameDef.Features);
            gameData.GameModes.Lookup.ConfigureDataFrom(this.gameDef.GameModes);
            gameData.Currencies.Lookup.ConfigureDataFrom(this.gameDef.Currencies, (def, data, created) => {
                if (created) {
                    data.Amount.Value = def.InitialAmount;
                }
            });
            gameData.Items.Lookup.ConfigureDataFrom(this.gameDef.Items);
            gameData.Quests.Lookup.ConfigureDataFrom(this.gameDef.Quests);
            gameData.QuestCounterTasks.Lookup.ConfigureDataFrom(this.gameDef.QuestCounterTasks);
            gameData.QuestDonateItemTasks.Lookup.ConfigureDataFrom(this.gameDef.QuestDonateItemTasks);
            gameData.Threshers.Lookup.ConfigureDataFrom(this.gameDef.Threshers);
            gameData.Gunsmiths.Lookup.ConfigureDataFrom(this.gameDef.Gunsmiths);
            gameData.ExpProgressionRewards.Lookup.ConfigureDataFrom(this.gameDef.ExpProgressionRewards);
            gameData.StoreItems.Lookup.ConfigureDataFrom(this.gameDef.StoreItems);
            gameData.CurrencyPurchases.Lookup.ConfigureDataFrom(this.gameDef.CurrencyPurchases);
            gameData.CoinFarms.Lookup.ConfigureDataFrom(this.gameDef.CoinFarms);

            await context.Execute(new EnsureInitialLoadoutCommand {
                LoadoutKey         = SharedConstants.Game.PlayerLoadouts.START,
                CreateNewIfNotExist = true,
                Safe                = default,
                TrashItems          = default,
            });

            await this.EnsureExpProgressionFeaturesUnlocked(context, gameData);

            return Ok;
        }

        private async Task EnsureExpProgressionFeaturesUnlocked(UserProfileServerCommandContext context, SdUserProfile gameData) {
            foreach (var expProgressionRewardDef in this.gameDef.ExpProgressionRewards.Items) {
                if (!gameData.ExpProgressionRewards.Get(expProgressionRewardDef.key).Claimed.Value) {
                    continue;
                }

                foreach (var rewardDef in expProgressionRewardDef.rewards) {
                    if (rewardDef is not FeatureRewardDef featureRewardDef) {
                        continue;
                    }

                    if (gameData.Features.Get(featureRewardDef.feature).Unlocked.Value) {
                        continue;
                    }

                    await context.Execute(new UserProfileOpenFeatureRewardCommand {
                        Reward = RewardBuildUtility.BuildFeature(featureRewardDef),
                    });
                }
            }
        }
    }
}