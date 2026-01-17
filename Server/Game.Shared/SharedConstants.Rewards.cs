namespace Game.Shared {
    public static partial class SharedConstants {
        public static class RewardTypes {
            public const string CURRENCY = "currency";
            public const string ITEM     = "item";
            public const string EXP      = "exp";
            public const string FEATURE  = "feature";
        }

        public static class Purchases {
            public const string CURRENCY_PURCHASE = "currency_purchase";
            public const string PURCHASE          = "iap_purchase";
        }

        public static class LootBoxTypes {
            public const string COMBINE_ONLY    = "combined";
            public const string CONGRATULATIONS = "congratulations";
        }
    }
}