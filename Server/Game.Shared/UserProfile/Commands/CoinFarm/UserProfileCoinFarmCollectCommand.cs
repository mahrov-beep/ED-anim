namespace Game.Shared.UserProfile.Commands.CoinFarm {
    using System;
    using System.Collections.Generic;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Balance;
    using Currencies;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileCoinFarmCollectCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string CoinFarmKey;
    }

    public class UserProfileCoinFarmCollectCommandHandler : UserProfileServerCommandHandler<UserProfileCoinFarmCollectCommand> {
        private readonly GameDef      gameDef;
        private readonly ITimeService timeService;

        public UserProfileCoinFarmCollectCommandHandler(GameDef gameDef, ITimeService timeService) {
            this.gameDef     = gameDef;
            this.timeService = timeService;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileCoinFarmCollectCommand command) {
            if (!this.gameDef.CoinFarms.TryGet(command.CoinFarmKey, out var coinFarmDef)) {
                return BadRequest("Invalid CoinFarm key, no def");
            }

            if (!gameData.CoinFarms.Lookup.TryGetValue(coinFarmDef.key, out var coinFarmData)) {
                return BadRequest("Invalid CoinFarm key, no data");
            }

            var coinFarmBalance = new CoinFarmBalance(this.gameDef, gameData, this.timeService);

            if (!coinFarmBalance.IsCollectAllowed(coinFarmDef.key)) {
                return BadRequest("Reward not ready yet");
            }

            var rewardAmount = coinFarmBalance.CalcCollectedRewardAmount(coinFarmDef.key, out var dueTime);

            await context.Execute(new UserProfileAddCurrencyCommand {
                CurrencyToAdd = new Dictionary<string, int> {
                    [coinFarmDef.CurrencyKey] = rewardAmount,
                },
            });

            coinFarmData.LastCollectTime.Value = dueTime;

            return Ok;
        }
    }
}