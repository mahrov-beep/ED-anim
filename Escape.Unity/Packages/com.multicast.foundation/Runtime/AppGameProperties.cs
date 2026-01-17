namespace Multicast {
    using GameProperties;

    public static class AppGameProperties {
        public static class Booleans {
            public static readonly BoolGamePropertyName HasSubscription            = "has_subscription";
            public static readonly BoolGamePropertyName ShowNothingBoostsInDetails = "show_nothing_boosts";
        }

        public static class Ints {
            public static readonly IntGamePropertyName Playtime          = "playtime";
            public static readonly IntGamePropertyName HoursSinceInstall = "hours_since_install";
        }
    }
}