namespace Game.Domain.Models.Purchases {
    using System.Collections.Generic;
    using Multicast;
    using Multicast.Collections;
    using Multicast.Numerics;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Store;
    using UniMob;

    public class CurrencyPurchasesModel : KeyedSingleInstanceModel<CurrencyPurchaseDef, SdCurrencyPurchase, CurrencyPurchaseModel> {
        public new List<CurrencyPurchaseModel> Values => base.Values;

        public CurrencyPurchasesModel(Lifetime lifetime, LookupCollection<CurrencyPurchaseDef> defs, SdUserProfile sdUserProfile)
            : base(lifetime, defs, sdUserProfile.CurrencyPurchases.Lookup) {
            this.AutoConfigureData = true;
        }
    }

    public class CurrencyPurchaseModel : Model<CurrencyPurchaseDef, SdCurrencyPurchase> {

        public CurrencyPurchaseModel(
            Lifetime lifetime,
            CurrencyPurchaseDef def,
            SdCurrencyPurchase data)
            : base(lifetime, def, data) {
        }

        public string PriceCurrency  => this.Def.priceCurrency;
        public string RewardCurrency => this.Def.rewardCurrency;

        public BigDouble Reward => this.Def.reward;

        public BigDouble Price => this.Def.price;

        [Atom] public bool IsAvailable => true;
    }
}