namespace Game.UI {
    using UniMob.UI.Widgets;

    public static partial class UiConstants {
        public static class Routes {
            public static readonly RouteSettings MainMenu = Fullscreen("main_menu");

            public static readonly RouteSettings ProgressScreen = Fullscreen("progress_screen");
            public static readonly RouteSettings BlackScreen    = Fullscreen("black_screen");
            public static readonly RouteSettings LoadingScreen  = Fullscreen("loading_screen");
            public static readonly RouteSettings FadeScreen     = Fullscreen("fade_screen");
            public static readonly RouteSettings BgScreen       = Fullscreen("bg_screen");
            public static readonly RouteSettings SearchGameScreen = Fullscreen("search_game_screen");

            public static readonly RouteSettings TraderShop           = Fullscreen("trader_shop");
            public static readonly RouteSettings Storage              = Fullscreen("storage");
            public static readonly RouteSettings GunsmithMenu         = Fullscreen("gunsmith_menu");
            public static readonly RouteSettings QuestsMenu           = Fullscreen("quests_menu");
            public static readonly RouteSettings ExpProgressionScreen = Fullscreen("exp_progression_screen");
            public static readonly RouteSettings LevelUp              = Fullscreen("level_up");
            public static readonly RouteSettings ThreshersMenu        = Fullscreen("threshers_menu");
            public static readonly RouteSettings GameModeSelection    = Fullscreen("game_modes_selection");
            public static readonly RouteSettings EditName             = Fullscreen("edit_name");
            public static readonly RouteSettings Store                = Fullscreen("store");
            public static readonly RouteSettings ItemInfo             = Fullscreen("item_info");
            public static readonly RouteSettings MailBoxMenu          = Fullscreen("mailbox_menu");

            public static readonly RouteSettings Friends              = Fullscreen("friends");
            public static readonly RouteSettings FriendsIncoming      = Fullscreen("friends_incoming");
            public static readonly RouteSettings FriendsAdd           = Fullscreen("friends_add");

            public static readonly RouteSettings GameInventory = Popup("game_inventory");
            public static readonly RouteSettings PartyInvite   = Popup("party_invite");
            public static readonly RouteSettings Settings      = Popup("settings_popup");

            public static RouteSettings TutorialPopup(string tutorialKey, string tutorialStep)
                => Popup($"tutorial_popup_{tutorialKey}_{tutorialStep}");

            private static RouteSettings Fullscreen(string name) => new RouteSettings(name, RouteModalType.Fullscreen);
            private static RouteSettings Popup(string name)      => new RouteSettings(name, RouteModalType.Popup);
        }
    }
}