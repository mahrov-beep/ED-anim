namespace Game.Shared.UserProfile.Data.Currencies {
    using System;
    using System.Collections.Generic;
    using Multicast.ServerData;

    public class SdCurrencyRepo : SdRepo<SdCurrency> {
        public SdCurrencyRepo(SdArgs args, Func<SdArgs, SdCurrency> factory) : base(args, factory) {
        }

        public bool HasEnough(Dictionary<string, int> currencies) {
            foreach (var (currencyKey, amount) in currencies) {
                if (!this.HasEnough(currencyKey, amount)) {
                    return false;
                }
            }

            return true;
        }

        public bool HasEnough(string currencyKey, int amount) {
            if (amount < 0) {
                return false;
            }

            if (!this.Lookup.TryGetValue(currencyKey, out var sdUserCurrency)) {
                return false;
            }

            if (sdUserCurrency.Amount.Value < amount) {
                return false;
            }

            return true;
        }
    }
}