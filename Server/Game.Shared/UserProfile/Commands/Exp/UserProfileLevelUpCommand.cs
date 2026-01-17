namespace Game.Shared.UserProfile.Commands.Exp {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Currencies;
    using Data;
    using Data.MailBox;
    using MailBox;
    using Rewards;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileLevelUpCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
    }

    public class UserProfileLevelUpCommandHandler : UserProfileServerCommandHandler<UserProfileLevelUpCommand> {
        private readonly GameDef gameDef;

        public UserProfileLevelUpCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileLevelUpCommand command) {
            var currentLevelDef = this.gameDef.GetLevel(gameData.Level.Value);

            if (gameData.Exp.Value < currentLevelDef.expToNextLevel) {
                return BadRequest("No enough exp to level up");
            }

            var nextLevelDef = this.gameDef.GetLevel(gameData.Level.Value + 1);
            if (nextLevelDef == currentLevelDef) {
                return BadRequest("At max level");
            }

            gameData.Level.Value += 1;

            gameData.Exp.Value -= currentLevelDef.expToNextLevel;

            foreach (var expProgressionRewardDef in this.gameDef.ExpProgressionRewards.Items) {
                if (expProgressionRewardDef.levelToComplete != currentLevelDef.level) {
                    continue;
                }

                gameData.ExpProgressionRewards.Get(expProgressionRewardDef.key).Claimed.Value = true;

                if (expProgressionRewardDef.rewards.Count == 0) {
                    continue;
                }

                var expProgressionRewardGuid = Guid.NewGuid().ToString();

                var items = expProgressionRewardDef.rewards.Where(it => it.type == SharedConstants.RewardTypes.ITEM).ToList();
                var notItems = expProgressionRewardDef.rewards.Where(it => it.type != SharedConstants.RewardTypes.ITEM).ToList();

                if (items.Count > 0) {
                    await context.Execute(new UserProfileMailBoxQueueRewardMessageCommand {
                        Type   = SdMailBoxMessageTypes.LootBoxReward,
                        Reward = RewardBuildUtility.BuildLootBox(SharedConstants.LootBoxTypes.COMBINE_ONLY, "exp_progression_reward", items),
                    });
                }
                
                if (notItems.Count > 0) {
                    await context.Execute(new UserProfileQueueRewardCommand {
                        RewardGuid = expProgressionRewardGuid,
                        Reward     = RewardBuildUtility.BuildLootBox(SharedConstants.LootBoxTypes.COMBINE_ONLY, "exp_progression_reward", notItems),
                    });
                    
                    await context.Execute(new UserProfileOpenRewardCommand {
                        RewardGuid = expProgressionRewardGuid,
                    });
                }
            }

            return Ok;
        }
    }
}