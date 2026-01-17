namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Multicast.RewardSystem;

    [Serializable, DDEObject, DDEImpl(typeof(RewardDef), "currency")]
    public class CurrencyRewardDef : RewardDef {
        [DDE("currency"), DDEExternalKey("Currencies")] public string currency;

        [DDE("amount")] public int amount;
    }
}