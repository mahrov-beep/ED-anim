namespace Game.Shared.UserProfile.Commands.Currencies {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using MessagePack;
    using Multicast;
    using Multicast.Numerics;
    using Rewards;

    [MessagePackObject]
    public class UserProfileDebugCheatAddCurrencyCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public Dictionary<string, int> CurrencyToAdd;
    }

    public class UserProfileDebugCheatAddCurrencyCommandHandler : UserProfileServerCommandHandler<UserProfileDebugCheatAddCurrencyCommand> {
        private readonly GameDef gameDef;

        public UserProfileDebugCheatAddCurrencyCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile data, UserProfileDebugCheatAddCurrencyCommand command) {
            var env = Environment.GetEnvironmentVariable("ENV");
            if (env != "dev" && env != "staging") {
                return BadRequest("Not available");
            }

            if (command.CurrencyToAdd == null) {
                return BadRequest("CurrencyTOAdd is null");
            }

            var rewards = command.CurrencyToAdd
                .Select(it => this.gameDef.Currencies.TryGet(it.Key, out var model) ? new { model, amount = it.Value } : null)
                .Where(it => it != null)
                .Select(it => Reward.Int(SharedConstants.RewardTypes.CURRENCY, it.model.key, it.amount))
                .ToArray();

            await context.Execute(new UserProfileQueueRewardCommand {
                Reward = Reward.LootBox(SharedConstants.LootBoxTypes.CONGRATULATIONS, "cheat_currencies", rewards),
            });

            return Ok;
        }
    }
}
