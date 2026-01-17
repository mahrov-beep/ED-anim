namespace Game.Shared.UserProfile.Commands.Rewards.Impl {
    using System.Collections.Generic;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Currencies;
    using Data;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileOpenCurrencyRewardCommand : IUserProfileServerCommand {
        [Key(0)] public Reward Reward;
    }

    public class UserProfileOpenCurrencyRewardCommandHandler : UserProfileServerCommandHandler<UserProfileOpenCurrencyRewardCommand> {
        private readonly GameDef gameDef;

        public UserProfileOpenCurrencyRewardCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileOpenCurrencyRewardCommand command) {
            var reward = command.Reward;

            if (!reward.ItemTypeIs(SharedConstants.RewardTypes.CURRENCY)) {
                return BadRequest("Not a currency drop");
            }

            if (!reward.AmountTypeIs(RewardAmountType.Int)) {
                return BadRequest("Currency drop must be with int amount");
            }

            if (!this.gameDef.Currencies.TryGet(reward.ItemKey, out var currencyDef)) {
                return BadRequest("Currency drop must be with valid currency key");
            }

            await context.Execute(new UserProfileAddCurrencyCommand {
                CurrencyToAdd = new Dictionary<string, int> {
                    [currencyDef.key] = reward.IntAmount,
                },
            });

            return Ok;
        }
    }
}