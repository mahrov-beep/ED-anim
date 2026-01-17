namespace Multicast.Purchasing {
    using System;
    using DirtyDataEditor;

    [Serializable, DDEObject]
    public class PurchaseDef : Def {
        [DDE("android_id", null)] public string androidID;
        [DDE("ios_id", null)]     public string iosID;

        [DDE("type")] public ProductType type;

        [DDE("price_usd_cents")] public int priceUdsCents;

        public double PriceUsd => this.priceUdsCents / 100.0;

        public enum ProductType {
            Consumable,
            NonConsumable,
            Subscription,
        }
    }
}