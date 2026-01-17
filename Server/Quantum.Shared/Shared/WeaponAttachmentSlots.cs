namespace Quantum {
    public enum WeaponAttachmentSlots {
        Invalid  = 0,
        Scope    = 1, // Прицелы
        Grip     = 2, // Рукояти
        Muzzle   = 3, // Ствол: компенсаторы и пламегасители
        Magazine = 4, // Магазин
        Stock    = 5, // Приклад
        Ammo     = 6, // Патроны
        Laser    = 7, // Лазеры
    }

    public static class WeaponAttachmentSlotsExtension {
        public const int WEAPON_ATTACHMENT_SLOTS = 8;

        public static readonly WeaponAttachmentSlots[] AllValidSlots;

        static WeaponAttachmentSlotsExtension() {
            AllValidSlots = EnumExt.MembersArrayExcluded(WeaponAttachmentSlots.Invalid);
        }
    }
}