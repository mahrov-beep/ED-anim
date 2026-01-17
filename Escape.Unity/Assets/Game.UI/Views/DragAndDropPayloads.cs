namespace Game.UI.Views {
    using Quantum;

    public abstract class DragAndDropPayloadItem {
    }

    [RequireFieldsInit] public sealed class DragAndDropPayloadItemEntityFromTetris : DragAndDropPayloadItem {
        public EntityRef ItemEntity;

        public string ItemGuid;

        public TetrisSource Source;
    }

    [RequireFieldsInit] public sealed class DragAndDropPayloadItemEntityFromSlot : DragAndDropPayloadItem {
        public EntityRef             ItemEntity;
        public CharacterLoadoutSlots SourceSlot;
    }

    [RequireFieldsInit] public sealed class DragAndDropPayloadItemEntityFromWeaponSlot : DragAndDropPayloadItem {
        public EntityRef             ItemEntity;
        public CharacterLoadoutSlots SourceSlot;
        public WeaponAttachmentSlots SourceWeaponAttachmentSlot;
    }

    [RequireFieldsInit] public sealed class DragAndDropPayloadItemFromTraderShopStorage : DragAndDropPayloadItem {
        public string ItemGuid;
    }
    [RequireFieldsInit] public sealed class DragAndDropPayloadItemFromTraderShopToSell : DragAndDropPayloadItem {
        public EntityRef ItemEntity;
        public string    ItemGuid;
    }

    [RequireFieldsInit] public sealed class DragAndDropPayloadItemFromTraderShopToBuy : DragAndDropPayloadItem {
        public string ItemGuid;
    }

    [RequireFieldsInit] public sealed class DragAndDropPayloadItemFromTraderShopItems : DragAndDropPayloadItem {
        public string ItemGuid;
    }
}