namespace Game.Shared {
    using System;
    using Multicast.FeatureToggles;

    public static partial class SharedConstants {
        public static class UrlRoutes {
            public static class Friends {
                public const string FRIEND_LIST       = "/friends/get-friends/";
                public const string INCOMING_REQUESTS = "/friends/get-incomings/";
                public const string ONLINE            = "/friends/get-online/";
                public const string ADD               = "/friends/add/";
                public const string ADD_BY_NICKNAME   = "/friends/add-by-nickname/";
                public const string ACCEPT            = "/friends/accept/";
                public const string DECLINE           = "/friends/decline/";
                public const string REMOVE            = "/friends/remove/";
                public const string INCOMING_BULK     = "/friends/incoming-bulk/";
            }

            public static class Party {
                public const string INVITE  = "/party/invite/";
                public const string ACCEPT  = "/party/accept/";
                public const string DECLINE = "/party/decline/";
                public const string LEAVE   = "/party/leave/";
                public const string KICK    = "/party/kick/";
                public const string START   = "/party/start/";
                public const string STATUS  = "/party/status/";
                public const string MAKE_LEADER = "/party/make-leader/";
                public const string READY_SET   = "/party/ready/set/";
            }

            public static class Auth {
                public const string GUEST = "/auth/guest";
            }

            public static class User {
                public const string DELETE = "/user/delete/";
                public const string GET_INFO = "/user/get-info/";
            }

            public static class Loadout {
                public const string GET_BY_USER = "/loadout/get-by-user/";
            }

            public static class Game {
                public const string REPORT_GAME_SNAPSHOT      = "/game/report-snapshot/";
                public const string REPORT_QUEST_COUNTER_TASK = "/game/quest-counter-task";
            }

            public static class Matchmaking {
                public const string JOIN   = "/api/matchmaking/join";
                public const string CANCEL = "/api/matchmaking/cancel";
                public const string STATUS = "/api/matchmaking/status";
            }

            public static class ServerEvents {
                public const string APP  = "/events/app/";
                public const string GAME = "/events/game/";
            }

            public const string USER_PROFILE    = "/user-profile/";
            public const string CHANGE_NICKNAME = "/user-profile/change-nickname/";

        }

        public static class Configs {
            // Core
            public const string FEATURE_TOGGLES = "Configs/*/feature_toggles";

            // Game
            public const string FEATURES                = "Configs/*/features";
            public const string CURRENCIES              = "Configs/*/currencies";
            public const string LEVELS                  = "Configs/*/levels";
            public const string GAME_MODES              = "Configs/*/game_modes";
            public const string ITEMS                   = "Configs/*/items";
            public const string ITEM_SETUPS             = "Configs/*/item_setups";
            public const string QUESTS                  = "Configs/*/quests";
            public const string QUEST_COUNTER_TASKS     = "Configs/*/quest_counter_tasks";
            public const string QUEST_DONATE_ITEM_TASKS = "Configs/*/quest_donateitem_tasks";
            public const string THRESHERS               = "Configs/*/threshers";
            public const string GUNSMITHS               = "Configs/*/gunsmiths";
            public const string GUNSMITH_LOADOUTS       = "Configs/*/gunsmith_loadouts";
            public const string PLAYER_LOADOUTS         = "Configs/*/player_loadouts";
            public const string CURRENCY_PURCHASES      = "Configs/*/currency_purchases";
            public const string STORE_ITEMS             = "Configs/*/store_items";
            public const string STORE_CATEGORIES        = "Configs/*/store_categories";
            public const string PURCHASES               = "Configs/*/purchases";
            public const string SOUND_EFFECTS           = "Configs/*/sound_effects";
            public const string EXP_PROGRESSION_REWARDS = "Configs/*/exp_progression_rewards";
            public const string TUTORIALS               = "Configs/*/tutorials";
            public const string COIN_FARMS              = "Configs/*/coin_farms";
        }

        public static class Game {
            public static class FeatureToggles {
                public static readonly FeatureToggleName ShowTutorials = "show_tutorials";
            }
            
            public static class Currencies {
                public const string BADGES          = "badges";
                public const string BUCKS           = "bucks";
                public const string CRYPT           = "crypt";
                public const string RATING          = "rating";
                public const string LOADOUT_TICKETS = "loadout_tickets";
            }

            public static class Threshers {
                public const string TRADER = "thresher_trader";
            }

            public static class Gunsmiths {
                public const string GUNSMITH_1 = "gunsmith_1";
            }

            public static class GunsmithLoadouts {
                public const string GUNSMITH_LOADOUT_DEFAULT = "gunsmith_loadout_A_1";
            }

            public static class PlayerLoadouts {
                public const string START = "player_loadout_start";
                public const string BASE  = "player_loadout_base";
            }

            public static class Features {
                public const string GUNSMITH     = "feature_gunsmith";
                public const string TRADER_SHOP  = "feature_trader_shop";
                public const string BLACK_MARKET = "feature_black_market";
            }

            public static class Tutorials {
                public const string FIRST_PLAY           = "tutorial_first_play";
                public const string GUNSMITH_BUY_LOADOUT = "tutorial_gunsmith_buy_loadout";
            }

            public static class Exp {
                public const string MATCH_PLAYED = "exp_match_played";
                public const string KILLS        = "exp_kills";
                public const string EARNINGS     = "exp_earnings";
            }

            public static class GameModes {
                public const string INIT_GAME_MODE = "factory";

                public const string FACTORY = "factory";
                public const string FACTORY_2_VS2 = "factory_2vs2";
            }

            public static class Quests {
                public const string FIRST_STEPS = "quest_first_steps";
            }

            public static class QuestCounterTasks {
                public const string PLAY_GAME = "quest_first_steps__play_game";
            }

            public static class CoinFarms {
                public const string COIN_FARM_BADGES = "coin_farm_badges";
            }

            public static class Items {
                public const string WEAPON_MACHINEGUN       = "item_weapon_machingun_rare";
                public const string WEAPON_AR               = "item_weapon_AR_rare";
                public const string WEAPON_AR_EPIC          = "item_weapon_AR_epic";
                public const string WEAPON_PP               = "item_weapon_pp_common";
                public const string WEAPON_SNIPER           = "item_weapon_sniper_rare";
                public const string WEAPON_PISTOL_COMMON    = "item_weapon_pistol_common";
                public const string WEAPON_KNIFE            = "item_weapon_knife";
                public const string EQUIP_BACKPACK_RARE     = "item_backpack_rare";
                public const string ATTACHMENT_SCOPE_HOLO   = "item_attachment_scope_common";
                public const string PERK_SPEED_UP           = "item_perk_speedUp";
                public const string HEAL_BOX_SMALL          = "item_healBox_small";
                public const string ABILITY_GRENADE         = "item_ability_grenade";                
                public const string SKIN_DEFAULT            = "item_skin_default";
                public const string ATTACHMENT_AMMO_PISTOL  = "item_ammoBox_pistol_common";
                public const string ATTACHMENT_AMMO_RIFFLE  = "item_ammoBox_rifle_common";
                public const string ATTACHMENT_AMMO_SHOTGUN = "item_ammoBox_shotgun_common";
            }
        }
    }
}