namespace Game.UI {
    using Multicast.GameProperties;

    public static class GameProperties {
        public static class Booleans {
            public static readonly BoolGamePropertyName DisableReconnect = "disable_reconnect";
            public static readonly BoolGamePropertyName ShowDevScenes    = "show_dev_scenes";
            public static readonly BoolGamePropertyName SkipMainMenu     = "skip_main_menu";

            public static readonly BoolGamePropertyName ForceUseMobileControls = "force_use_mobile_controls";

            public static readonly BoolGamePropertyName IgnoreTraderShopBlockers = "ignore_trader_shop_blockers";
        }
    }
}