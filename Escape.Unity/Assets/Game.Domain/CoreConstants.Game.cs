namespace Game.Domain {
    using System;
    using Shared;

    public static partial class CoreConstants {
        public static Func<GameDef> GameDefAccessEditorOnly;

        public static class Scenes {
            public const string EMPTY                           = "scenes/Empty";
            public const string MAIN_MENU                       = "scenes/MainMenu";
            public const string MAIN_MANU_STORAGE_ADDITIVE      = "scenes/MainMenuStorageAdditive";
            public const string MAIN_MANU_GAME_RESULTS_ADDITIVE = "scenes/MainMenuGameResultsAdditive";
        }
        
        public static class SoundEffectKeys {
            public const string Button     = "Button";
            public const string Equip      = "Equip";
            public const string Sell       = "Sell";
            public const string Buy        = "Buy";
            public const string USE_KIT    = "UseKit";
            public const string LEVEL_UP   = "LevelUp";
            public const string DRAG_ITEM  = "DragItem";
            public const string DEAL_ITEM  = "DealItem";
            public const string TAKE_ALL   = "TakeAll";
            public const string EQUIP_BEST = "EquipBest";
            public const string THROW_ITEM = "ThrowItem";
            public const string QUEST_DONE = "QuestDone";
            public const string HIT_MARK   = "HitMark";
        }

        public static class Quantum {
            public static class GameModeAssets {
                public const string MAIN_MENU_STORAGE      = "QuantumUser/Resources/Configs/GameModes/MainMenuStorage";
                public const string MAIN_MENU_GAME_RESULTS = "QuantumUser/Resources/Configs/GameModes/MainMenuGameResults";
            }
        }
        
        public static class Game {
            public static class ItemQualityVisual {
                public const string LIGHT      = "light";
                public const string ADVANCED   = "advanced";
                public const string DRAGON_SKIN = "dragonskin";
            }
        }

        public static class Tetris {
            public static int CELL_SIZE = 100;
        }
    }
}