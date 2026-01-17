namespace Quantum {
    using System;
    using System.Linq;

    public enum CharacterLoadoutSlots {
        Invalid         = 0,
        MeleeWeapon     = 1,
        PrimaryWeapon   = 2,
        SecondaryWeapon = 3,
        Backpack        = 4,
        Helmet          = 5,
        Armor           = 6,
        Skin            = 7,
        Skill           = 8,
        Perk1           = 9,
        Perk2           = 10,
        Perk3           = 11,
        Headphones      = 12,
        Safe            = 13,
    }

    public enum TetrisSource : byte {
        Inventory = 0,
        Safe      = 1,
        Storage   = 2,
    }
    
    public static class CharacterLoadoutSlotsExtension {
        public const int CHARACTER_LOADOUT_SLOTS = 14;

        public static readonly CharacterLoadoutSlots[] AllValidSlots;
        public static readonly CharacterLoadoutSlots[] AllValidSlotsBackpackFirst;
        public static readonly CharacterLoadoutSlots[] AllValidSlotsExceptBackpack;
        public static readonly CharacterLoadoutSlots[] WeaponSlots;
        public static readonly CharacterLoadoutSlots[] NonWeaponSlots;
        public static readonly CharacterLoadoutSlots[] UsedForQualitySlots;

        static CharacterLoadoutSlotsExtension() {
            AllValidSlots = EnumExt.MembersArrayExcluded(CharacterLoadoutSlots.Invalid);
            AllValidSlotsBackpackFirst = AllValidSlots
                .Where(it => it != CharacterLoadoutSlots.Backpack)
                .Prepend(CharacterLoadoutSlots.Backpack)
                .ToArray();

            AllValidSlotsExceptBackpack = AllValidSlots
                .Where(it => it != CharacterLoadoutSlots.Backpack)
                .ToArray();
            
            WeaponSlots    = new[] { CharacterLoadoutSlots.MeleeWeapon, CharacterLoadoutSlots.PrimaryWeapon, CharacterLoadoutSlots.SecondaryWeapon };
            NonWeaponSlots = AllValidSlots.Except(WeaponSlots).ToArray();

            UsedForQualitySlots = AllValidSlots;
        }

        public static bool IsWeaponSlot(CharacterLoadoutSlots slot) {
            return Array.IndexOf(WeaponSlots, slot) != -1;
        }
    }
}