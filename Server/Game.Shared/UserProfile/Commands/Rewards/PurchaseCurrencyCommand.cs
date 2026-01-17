namespace Game.Shared.UserProfile.Commands.Rewards {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Currencies;
    using MessagePack;
    using Multicast;
    using Multicast.Numerics;
    using Shared;
    using UserProfile;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class PurchaseCurrencyCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public Dictionary<string, int> CurrenciesToBuy;
        [Key(1)] public Dictionary<string, int> CurrenciesToTake;
    }

    public class PurchaseCurrencyCommandHandler : UserProfileServerCommandHandler<PurchaseCurrencyCommand> {
        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, PurchaseCurrencyCommand command) {
            await context.Execute(new UserProfileCurrencyTakeAmountCommand {
                CurrencyToTake = command.CurrenciesToTake,
            });

            Reward[] rewards = new Reward[command.CurrenciesToBuy.Count];

            var i = 0;
            
            foreach (var currency in command.CurrenciesToBuy) {
                rewards[i++] = Reward.Int(SharedConstants.RewardTypes.CURRENCY, currency.Key, currency.Value);
            }

            var reward = Reward.LootBox(SharedConstants.LootBoxTypes.CONGRATULATIONS, SharedConstants.RewardTypes.CURRENCY, rewards);

            var rewardGuid = Guid.NewGuid().ToString();

            await context.Execute(new UserProfileQueueRewardCommand {
                RewardGuid = rewardGuid,
                Reward     = reward,
            });
            
            await context.Execute(new UserProfileOpenRewardCommand {
                RewardGuid = rewardGuid,
            });
            
            return Ok;
        }
    }
}