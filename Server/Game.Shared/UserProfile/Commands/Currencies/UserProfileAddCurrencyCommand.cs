namespace Game.Shared.UserProfile.Commands.Currencies {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using MessagePack;
    using Multicast;

    [MessagePackObject]
    public class UserProfileAddCurrencyCommand : IUserProfileServerCommand {
        [Key(0)] public Dictionary<string, int> CurrencyToAdd;
    }

    public class UserProfileAddCurrencyCommandHandler : UserProfileServerCommandHandler<UserProfileAddCurrencyCommand> {
        private readonly GameDef gameDef;

        public UserProfileAddCurrencyCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile data, UserProfileAddCurrencyCommand command) {
            if (command.CurrencyToAdd == null) {
                return BadRequest("CurrencyToAdd is null");
            }

            foreach (var (currencyKey, amountToAdd) in command.CurrencyToAdd) {
                if (!this.gameDef.Currencies.TryGet(currencyKey, out var currencyDef)) {
                    return BadRequest("Invalid currency key");
                }

                if (!data.Currencies.Lookup.TryGetValue(currencyKey, out var sdUserCurrency)) {
                    return InternalError("Currency does not exist in user data");
                }

                if (currencyDef.AllowNegativeAdditions == false && amountToAdd < 0) {
                    return BadRequest("Amount must be greater that zero");
                }

                sdUserCurrency.Amount.Value += amountToAdd;

                if (currencyDef.MinAmount is { } minAmount) {
                    sdUserCurrency.Amount.Value = Math.Max(sdUserCurrency.Amount.Value, minAmount);
                }
            }

            return Ok;
        }
    }
}