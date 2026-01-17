namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Multicast.Numerics;

    [Serializable, DDEObject]
    public class CurrencyPurchaseDef : Def {
        [DDE("price_currency", null)] public string priceCurrency;

        [DDE("price")] public BigDouble price;

        [DDE("reward_currency", null)] public string rewardCurrency;

        [DDE("reward")] public BigDouble reward;
    }
}