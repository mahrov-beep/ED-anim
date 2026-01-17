namespace Game.UI.Widgets.GameInventory {
    using Quantum;

    public static class LoadoutSlotExtensions {
        public static CharacterLoadoutSlots[] VisibleSlots = new[] {
            CharacterLoadoutSlots.PrimaryWeapon,
            // CharacterLoadoutSlots.MeleeWeapon,
            CharacterLoadoutSlots.SecondaryWeapon,

            CharacterLoadoutSlots.Helmet,
            CharacterLoadoutSlots.Armor,
            CharacterLoadoutSlots.Backpack,
            CharacterLoadoutSlots.Headphones,

            CharacterLoadoutSlots.Skill,
            //CharacterLoadoutSlots.Skin,
            CharacterLoadoutSlots.Perk1,
            CharacterLoadoutSlots.Perk2,
            CharacterLoadoutSlots.Perk3,
        };

        public static LoadoutSlotVisual GetVisual(this CharacterLoadoutSlots slot) => slot switch {
            CharacterLoadoutSlots.MeleeWeapon => LoadoutSlotVisual.MeleeWeapon,
            CharacterLoadoutSlots.PrimaryWeapon => LoadoutSlotVisual.PrimaryWeapon,
            CharacterLoadoutSlots.SecondaryWeapon => LoadoutSlotVisual.SecondaryWeapon,

            CharacterLoadoutSlots.Helmet => LoadoutSlotVisual.Default,
            CharacterLoadoutSlots.Armor => LoadoutSlotVisual.Default,
            CharacterLoadoutSlots.Backpack => LoadoutSlotVisual.Default,
            CharacterLoadoutSlots.Headphones => LoadoutSlotVisual.Default,
            CharacterLoadoutSlots.Safe => LoadoutSlotVisual.Default,

            //CharacterLoadoutSlots.Skin => LoadoutSlotVisual.Mini,
            CharacterLoadoutSlots.Skill => LoadoutSlotVisual.Mini,
            CharacterLoadoutSlots.Perk1 => LoadoutSlotVisual.Mini,
            CharacterLoadoutSlots.Perk2 => LoadoutSlotVisual.Mini,
            CharacterLoadoutSlots.Perk3 => LoadoutSlotVisual.Mini,

            _ => LoadoutSlotVisual.Default,
        };
    }

    public enum LoadoutSlotVisual {
        Default,
        Mini,
        PrimaryWeapon,
        SecondaryWeapon,
        MeleeWeapon,
    }
}