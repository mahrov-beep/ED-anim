namespace Game.UI.Controllers.Features.Store {
    using System;
    using Multicast;

    public static class StoreFeatureEvents {
        public static readonly EventSource Open  = new();
        public static readonly EventSource Close = new();

        public static readonly EventSource<PurchaseArgs> Purchase = new();

        [Serializable, RequireFieldsInit]
        public struct PurchaseArgs {
            public string storeItemKey;
        }
    }
}