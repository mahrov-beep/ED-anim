namespace Game.Shared.UserProfile.Commands.Currencies {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using MessagePack;
    using Multicast;

    [MessagePackObject]
    public class UserProfileCurrencyTakeAmountCommand : IUserProfileServerCommand {
        [Key(0)] public Dictionary<string, int> CurrencyToTake;
    }

    public class UserProfileCurrencyTakeAmountCommandHandler : UserProfileServerCommandHandler<UserProfileCurrencyTakeAmountCommand> {
        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile data, UserProfileCurrencyTakeAmountCommand command) {
            if (command.CurrencyToTake == null) {
                return BadRequest("CurrencyToTake is null");
            }

            if (!data.Currencies.HasEnough(command.CurrencyToTake)) {
                return BadRequest("No enough currency to take");
            }

            foreach (var (currencyKey, amountToTake) in command.CurrencyToTake) {
                if (!data.Currencies.Lookup.TryGetValue(currencyKey, out var sdUserCurrency)) {
                    return BadRequest("invalid currency key");
                }

                if (amountToTake < 0) {
                    return BadRequest("Amount must be greater than zero");
                }

                sdUserCurrency.Amount.Value = Math.Max(0, sdUserCurrency.Amount.Value - amountToTake);
            }

            return Ok;
        }
    }
}