namespace Game.Domain.Currencies {
    using Multicast;
    using Multicast.Collections;
    using Multicast.Numerics;
    using Shared;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Currencies;
    using UniMob;

    public class CurrenciesModel : KeyedSingleInstanceModel<CurrencyDef, SdCurrency, CurrencyModel> {
        public CurrenciesModel(Lifetime lifetime, LookupCollection<CurrencyDef> defs, SdUserProfile userProfile)
            : base(lifetime, defs, userProfile.Currencies.Lookup) {
            this.AutoConfigureData = true;
        }

        [Atom] public CurrencyModel Rating => this.Get(SharedConstants.Game.Currencies.RATING);

        public bool HasEnough(Cost cost) {
            foreach (var (currencyKey, amount) in cost) {
                if (!this.Get(currencyKey).HasEnough(amount)) {
                    return false;
                }
            }

            return true;
        }
    }

    public class CurrencyModel : Model<CurrencyDef, SdCurrency> {
        public CurrencyModel(Lifetime lifetime, CurrencyDef def, SdCurrency data) : base(lifetime, def, data) {
        }

        [Atom] public int Amount => this.Data.Amount.Value;

        public bool HasEnough(BigDouble cost) => this.Amount >= cost;
    }
}