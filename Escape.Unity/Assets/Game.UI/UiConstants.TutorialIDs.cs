namespace Game.UI {
    using Multicast.Misc.Tutorial;
    using Shared;

    public static partial class UiConstants {
        [TutorialIDsContainer]
        public static class TutorialIDs {
            public static class MainMenu {
                public static readonly TutorialObjectID PlayButton     = Make("main_menu__play_button");
                public static readonly TutorialObjectID PlayButtonHelp = Make("main_menu__play_button_help");

                public static readonly TutorialObjectID GunsmithButton     = Make("main_menu__gunsmith_button");
                public static readonly TutorialObjectID GunsmithButtonHelp = Make("main_menu__gunsmith_button_help");
            }

            public static class GameModeSelector {
                public static readonly TutorialObjectID ConfirmButton     = Make("game_mode_selection__confirm");
                public static readonly TutorialObjectID ConfirmButtonHelp = Make("game_mode_selection__confirm_help");
                public static readonly TutorialObjectID Details           = Make("game_mode_selection__details");

                public static readonly TutorialObjectID SelectModeFactory     = Make("game_mode_selection__mode_button", SharedConstants.Game.GameModes.FACTORY);
                public static readonly TutorialObjectID SelectModeFactoryHelp = Make("game_mode_selection__mode_button_help", SharedConstants.Game.GameModes.FACTORY);
            }

            public static class GunsmithMenu {
                public static readonly TutorialObjectID AllLoadouts = Make("gunsmith_menu__all_loadouts");
                public static readonly TutorialObjectID Close       = Make("gunsmith_menu__close");
                public static readonly TutorialObjectID CloseHelp   = Make("gunsmith_menu__close_help");
            }

            private static TutorialObjectID Make(string primary, string secondary = "") {
                return new TutorialObjectID(primary, secondary);
            }
        }
    }
}