namespace Game.Shared.Defs {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [Serializable, DDEObject]
    public class CoinFarmDef : Def {
        [DDE("currency_key"), DDEExternalKey("Currencies")]
        public string CurrencyKey;

        [DDE("thresher_key"), DDEExternalKey("Threshers")]
        public string ThresherKey;

        [DDE("produce_quantity")]         public List<int> ProduceQuantity;
        [DDE("produce_interval_seconds")] public List<int> ProduceIntervalSeconds;

        [DDE("storage_capacity")] public List<int> StorageCapacity;

        [DDE("locked_by_feature"), DDEExternalKey("Features")]
        public string LockedByFeatureKey;
    }
}