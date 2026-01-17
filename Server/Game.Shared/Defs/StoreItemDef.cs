namespace Game.Shared.Defs {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Multicast.RewardSystem;

    [Serializable, DDEObject]
    public class StoreItemDef : Def {
        [DDE("item_type")] public StoreItemType itemType;

        [DDE("category"), DDEExternalKey("StoreCategories")] public string category;

        [DDE("store_group", null), CanBeNull]               public string storeGroup;
        [DDE("ui_prefab", null), DDEAddressable, CanBeNull] public string uiPrefab;

        [DDE("iap_purchase_key", null), CanBeNull, DDEExternalKey("Purchases")]
        [DDENonNullWhen(nameof(itemType), StoreItemType.IapPurchase)]
        [DDENonNullWhen(nameof(itemType), StoreItemType.IapWithBonusPurchase)]
        [DDENonNullWhen(nameof(itemType), StoreItemType.KeyIapPurchase)]
        [DDENonNullWhen(nameof(itemType), StoreItemType.IapTimedCard)]
        public string iapPurchaseKey;

        [DDE("currency_purchase_key", null), CanBeNull, DDEExternalKey("CurrencyPurchases")]
        [DDENonNullWhen(nameof(itemType), StoreItemType.CurrencyPurchase)]
        public string currencyPurchaseKey;

        [DDE("extra_drops", DDE.Empty)] public List<RewardDef> extraDrops;
    }

    public enum StoreItemType {
        None = 0,

        CurrencyPurchase     = 1,
        IapPurchase          = 2,
        IapWithBonusPurchase = 3,
        KeyIapPurchase       = 4,
        Gift                 = 5,
        PowerUp              = 6,
        IapTimedCard         = 7,
        BundleIapPurchase    = 8,
    }
}