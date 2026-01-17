namespace Game.UI {
    using Multicast;
    using UniMob.UI;

    public static partial class UiConstants {
        [WidgetViewReferenceContainer]
        public static class Views {
            public static WidgetViewReference BlackOverlay = Make("ui/Black Overlay View");

            public static WidgetViewReference LoadingScreen            = Make("ui/Loading Screen");
            public static WidgetViewReference LoadingFadeScreen        = Make("ui/Loading Fade Screen");
            public static WidgetViewReference LoadingBlackScreen       = Make("ui/Loading Black Screen");
            public static WidgetViewReference LoadingBgScreen          = Make("ui/Loading Bg Screen");
            public static WidgetViewReference SearchGameScreen         = Make("ui/SearchGame Screen");
            public static WidgetViewReference ProgressScreen           = Make("ui/Progress Screen");
            public static WidgetViewReference FloatNotificationsScreen = Make("ui/Float Notifications Screen");
            public static WidgetViewReference ScreenView               = Make("ui/Screen View");

            public static readonly WidgetViewReference Popup = Make("ui/Popup View");

            public static readonly WidgetViewReference ConfirmationScreen = Make("ui/Confirmation Screen View");

            public static class Party {
                public static readonly WidgetViewReference PartyStatus  = Make("ui/Party/Party Status View");
                public static readonly WidgetViewReference PartyMember = Make("ui/Party/Party Member View");
            }
            
            public static class Tutorial {
                public static readonly WidgetViewReference MaskScreen  = Make("ui/Tutorial/Tutorial View");
                public static readonly WidgetViewReference PopupScreen = Make("ui/Tutorial/Tutorial Popup View");
            }

            public static class Purchases {
                public static readonly WidgetViewReference Store                 = Make("ui/Purchases/Store");
                public static readonly WidgetViewReference StoreCategory         = Make("ui/Purchases/Purchases Store Category");
                public static readonly WidgetViewReference TopUpView             = Make("ui/Purchases/Purchases Top Up View");
                public static readonly WidgetViewReference PurchaseNotEnoughView = Make("ui/Purchases/Purchase Not Enough View");


                public static class Items {
                    public static readonly WidgetViewReference Iap      = Make("ui/Purchases/Items/Purchases Iap Item");
                    public static readonly WidgetViewReference Currency = Make("ui/Purchases/Items/Purchases Currency Item");
                    public static readonly WidgetViewReference KeyIap   = Make("ui/Purchases/Items/Purchases Key Iap Item");
                }

                public static class Drops {
                    public static readonly WidgetViewReference Currency    = Make("ui/Purchases/Drops/Purchases Currency Drop");
                    public static readonly WidgetViewReference CrystalPack = Make("ui/Purchases/Drops/Purchases Crystal Pack Drop");
                }
            }

            public static class Subscription {
                public static readonly WidgetViewReference RestoredPurchasesView = Make("ui/Subscription/Restored Purchases View");
            }

            public static class Alert {
                public static WidgetViewReference Dialog         = Make("ui/Alert/Alert Dialog");
                public static WidgetViewReference ButtonPositive = Make("ui/Alert/Alert Dialog Button Positive");
                public static WidgetViewReference ButtonNegative = Make("ui/Alert/Alert Dialog Button Negative");
            }

            public static class EditName {
                public static WidgetViewReference Dialog = Make("ui/EditName/EditNameDialog");
            }

            public static class Friends {
                public static WidgetViewReference Panel     = Make("ui/Friends/Friends Panel");
                public static WidgetViewReference ListItem  = Make("ui/Friends/Friends List Item");
                public static WidgetViewReference AddFriend = Make("ui/Friends/AddFriend Panel");
                public static WidgetViewReference Incoming = Make("ui/Friends/Incoming Panel");
            }

            public static class Header {
                public static WidgetViewReference Screen = Make("ui/Header/Header");

                public static WidgetViewReference HeaderCurrency = Make("ui/Header/Header Currency");
            }

            public static class Settings {
                public static WidgetViewReference Graphics = Make("ui/Settings/Graphics Settings View");
            }

            public static class MainMenu {
                public static WidgetViewReference Screen     = Make("ui/MainMenu/Main Menu");
                public static WidgetViewReference PlayButton = Make("ui/MainMenu/MainMenuPlayButton");
                public static WidgetViewReference PlayButtonCancel = Make("ui/MainMenu/MainMenuPlayButtonCancel");
            }

            public static class QuestMenu {
                public static WidgetViewReference Screen = Make("ui/QuestMenu/Quest Menu");

                public static WidgetViewReference ListItem     = Make("ui/QuestMenu/Quest Menu List Item");
                public static WidgetViewReference QuestDetails = Make("ui/QuestMenu/Quest Menu Quest Details");

                public static WidgetViewReference CounterTaskActive    = Make("ui/QuestMenu/Quest Menu Counter Task Active");
                public static WidgetViewReference CounterTaskCompleted = Make("ui/QuestMenu/Quest Menu Counter Task Completed");
                public static WidgetViewReference CounterTaskLocked    = Make("ui/QuestMenu/Quest Menu Counter Task Locked");

                public static WidgetViewReference DonateItemTaskActive  = Make("ui/QuestMenu/Quest Menu DonateItem Task Active v2");
                public static WidgetViewReference DonateItemTaskDonated = Make("ui/QuestMenu/Quest Menu DonateItem Task Donated v2");
                public static WidgetViewReference DonateItemTaskLocked  = Make("ui/QuestMenu/Quest Menu DonateItem Task Locked v2");
            }

            public static class LevelUp {
                public static WidgetViewReference Screen = Make("ui/LevelUp/Level Up View");
            }

            public static class GameModes {
                public static WidgetViewReference Screen = Make("ui/GameModes/Game Modes Screen");

                public static WidgetViewReference Item    = Make("ui/GameModes/Game Mode item");
                public static WidgetViewReference Details = Make("ui/GameModes/Game Mode Details");
            }

            public static class Storage {
                public static WidgetViewReference Screen = Make("ui/Storage/Storage");

                public static WidgetViewReference Item   = Make("ui/Storage/Storage Item");
                public static WidgetViewReference ItemX2 = Make("ui/Storage/Storage Item X2");

                public static WidgetViewReference ItemBlocker     = Make("ui/Storage/Storage Item Blocker");
                public static WidgetViewReference ItemPartialFill = Make("ui/Storage/Storage Item Partial Fill");
            }

            public static class TraderShop {
                public static WidgetViewReference Screen = Make("ui/TraderShop/Trader Shop Screen");
            }

            public static class RewardsLarge {
                public static WidgetViewReference Currency = Make("ui/RewardsLarge/Reward Large Currency");
                public static WidgetViewReference Item     = Make("ui/RewardsLarge/Reward Large Item");
                public static WidgetViewReference Feature  = Make("ui/RewardsLarge/Reward Large Feature");
            }

            public static class ExpProgressionRewards {
                public static WidgetViewReference Screen     = Make("ui/ExpProgressionRewards/Exp Progression Rewards Screen");
                public static WidgetViewReference Item       = Make("ui/ExpProgressionRewards/Exp Progression Reward Item");
                public static WidgetViewReference Background = Make("ui/ExpProgressionRewards/Exp Progression Rewards Background");
            }

            public static class ItemInfo {
                public static WidgetViewReference Screen = Make("ui/ItemInfo/ItemInfo Screen");

                public static WidgetViewReference Stat = Make("ui/ItemInfo/ItemInfo Stat");
            }

            public static class Quests {
                public static WidgetViewReference Quest = Make("ui/Quests/Quest");

                public static WidgetViewReference CounterTaskActive    = Make("ui/Quests/Quest Counter Task Active");
                public static WidgetViewReference CounterTaskCompleted = Make("ui/Quests/Quest Counter Task Completed");

                public static WidgetViewReference DonateItemTaskActive    = Make("ui/Quests/Quest DonateItem Task Active v3");
                public static WidgetViewReference DonateItemTaskCompleted = Make("ui/Quests/Quest DonateItem Task Completed v3");
            }

            public static class Threshers {
                public static WidgetViewReference Screen  = Make("ui/Thresher/Threshers Menu Screen");
                public static WidgetViewReference Details = Make("ui/Thresher/Thresher View");
                public static WidgetViewReference Item    = Make("ui/Thresher/Thresher item");
            }

            public static class Gunsmiths {
                public static WidgetViewReference Screen = Make("ui/Gunsmiths/Gunsmith Menu Screen");

                public static WidgetViewReference Loadout = Make("ui/Gunsmiths/Gunsmith Loadout");

                public static WidgetViewReference LevelBlock = Make("ui/Gunsmiths/Gunsmith Loadout Level Block");
            }

            public static class MailBox {
                public static WidgetViewReference Screen = Make("ui/MailBox/MailBox Menu Screen");
                
                public static WidgetViewReference Message = Make("ui/MailBox/MailBox Message");
            }

            public static class Game {
                public static WidgetViewReference Screen = Make("ui/Game/Game");

                public static WidgetViewReference NearbyItemsOpened         = Make("ui/Game/Game Nearby Items Opened");
                public static WidgetViewReference NearbyItemsClosed         = Make("ui/Game/Game Nearby Items Closed");
                public static WidgetViewReference NearbyItemBox             = Make("ui/Game/Game Nearby ItemBox");
                public static WidgetViewReference NearbyItemBoxBackpack     = Make("ui/Game/Game Nearby ItemBox (Backpack variant)");
                public static WidgetViewReference NearbyInteractiveZoneExit = Make("ui/Game/Game Nearby Interactive Zone Exit");

                public static WidgetViewReference EscapeModeSummary = Make("ui/Game/Game Escape Mode Summary");
            }

            public static class Items {
                public static WidgetViewReference AttachmentMarker      = Make("ui/Items/Item Attachment Marker");
                public static WidgetViewReference EmptyAttachmentMarker = Make("ui/Items/Empty Item Attachment Marker");

                public static WidgetViewReference GroupingSeparator = Make("ui/Items/Item Grouping Separator");

                public static WidgetViewReference CurrencyItem = Make("ui/Items/Currency Item");
                public static WidgetViewReference ExpItem      = Make("ui/Items/Exp Item");
                public static WidgetViewReference FeatureItem  = Make("ui/Items/Feature Item");
            }

            public static class GameInventory {
                public static WidgetViewReference Screen       = Make("ui/GameInventory/Game Inventory Screen");
                public static WidgetViewReference Cell         = Make("ui/GameInventory/Game Inventory Cell");
                public static WidgetViewReference CellWithItem = Make("ui/GameInventory/Game Inventory Cell With Item");
                public static WidgetViewReference Tetris       = Make("ui/GameInventory/Game Inventory Tetris");

                public static WidgetViewReference TrashItem              = Make("ui/GameInventory/Game Inventory Trash Item");
                public static WidgetViewReference TrashItemMini          = Make("ui/GameInventory/Game Inventory Trash Item Mini");
                public static WidgetViewReference EmptySlotItem          = Make("ui/GameInventory/Empty Slot Item");
                public static WidgetViewReference TrashButtonItem        = Make("ui/GameInventory/Game Inventory Trash Button Item");
                public static WidgetViewReference SlotEmptyItem          = Make("ui/GameInventory/Game Inventory Empty Slot Item");
                public static WidgetViewReference SlotEmptyItemMini      = Make("ui/GameInventory/Game Inventory Empty Slot Item Mini");
                public static WidgetViewReference SlotEmptyItemPrimary   = Make("ui/GameInventory/Game Inventory Empty Slot Item Primary");
                public static WidgetViewReference SlotEmptyItemSecondary = Make("ui/GameInventory/Game Inventory Empty Slot Item Secondary");
                public static WidgetViewReference SlotEmptyItemMelee     = Make("ui/GameInventory/Game Inventory Empty Slot Item Melee");
                public static WidgetViewReference SlotItem               = Make("ui/GameInventory/Game Inventory Slot Item");
                public static WidgetViewReference SafeSlotItem           = Make("ui/GameInventory/Game Inventory Slot Item");
                public static WidgetViewReference SlotItemMini           = Make("ui/GameInventory/Game Inventory Slot Item Mini");
                public static WidgetViewReference SlotItemPrimary        = Make("ui/GameInventory/Game Inventory Slot Item Primary");
                public static WidgetViewReference SlotItemSecondary      = Make("ui/GameInventory/Game Inventory Slot Item Secondary");
                public static WidgetViewReference SlotItemMelee          = Make("ui/GameInventory/Game Inventory Slot Item Melee");
                public static WidgetViewReference InventoryItemFilter    = Make("ui/GameInventory/Game Inventory Item Filter");

                public static WidgetViewReference WeaponAttachmentSlotEmpty = Make("ui/GameInventory/Game Inventory Empty Weapon Attachment Slot Item");
                public static WidgetViewReference WeaponAttachmentSlot      = Make("ui/GameInventory/Game Inventory Weapon Attachment Slot Item");
            }

            public static class HUD {
                public static WidgetViewReference SelectableWeapon      = Make("ui/Game/Hud/Selectable Weapon View");
                public static WidgetViewReference SelectableMeleeWeapon = Make("ui/Game/Hud/Selectable Melee Weapon View");
                public static WidgetViewReference UnitAbility           = Make("ui/Game/Hud/Unit Ability View");
                public static WidgetViewReference UnitAbilityKnifeAttack = Make("ui/Game/Hud/Unit Ability View (KnifeAttack variant)");
                public static WidgetViewReference UnitAbilityRevive     = Make("ui/Game/Hud/Unit Ability View (Revive variant)");
                public static WidgetViewReference Map                   = Make("ui/Game/Hud/Map View");
                public static WidgetViewReference ListenedCue           = Make("ui/Game/Hud/ListenedCue View");
                public static WidgetViewReference DamageCue             = Make("ui/Game/Hud/DamageCue View");
                public static WidgetViewReference Health                = Make("ui/Game/Hud/Health View");
                public static WidgetViewReference Vignette              = Make("ui/Game/Hud/Vignette View");
                public static WidgetViewReference GrenadeIndicator      = Make("ui/Game/Hud/GrenadeIndicator View");
                public static WidgetViewReference WeaponAmmoOption      = Make("ui/Game/Hud/Weapon Ammo Option");
            }

            public static class GameResults {
                public static class Simple {
                    public static WidgetViewReference Screen = Make("ui/GameResults/Simple/Simple Game Results Screen");
                }
            }

            public static class World {
                public static WidgetViewReference HealthBarView    = Make("ui/Header/Unit Health Bar View");
                public static WidgetViewReference UnitPartyView    = Make("ui/Header/Unit Party View");
                public static WidgetViewReference ItemBoxTimerView = Make("ui/Header/Item Box Timer View");
                public static WidgetViewReference HitMarkView      = Make("ui/Header/Hit Mark View");
                public static WidgetViewReference DebuffSlowView   = Make("ui/Header/Debuff Slow View");
                public static WidgetViewReference DynamicAimView   = Make("ui/Aims/Dynamic Aim View");
            }

            private static WidgetViewReference Make(string path) {
                return WidgetViewReference.Addressable(path);
            }
        }
    }
}
