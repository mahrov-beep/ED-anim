using Quantum;

namespace Game.Domain.GameInventory {
    using global::System;
    using global::System.Collections.Generic;
    using JetBrains.Annotations;
    using Multicast;
    using UniMob;

    public class GameInventoryWeaponAttachmentsModel : Model {
        private readonly AtomEntityRefList<GameInventoryWeaponAttachmentSlotItemModel> slotItems;

        public GameInventoryWeaponAttachmentsModel(Lifetime lifetime) : base(lifetime) {
            this.slotItems = new AtomEntityRefList<GameInventoryWeaponAttachmentSlotItemModel>(lifetime, 
                () => new GameInventoryWeaponAttachmentSlotItemModel(), it => ref it.Frame, it => ref it.ItemEntity);
        }

        public bool TryGetSlotItem(WeaponAttachmentSlots slotType, out GameInventoryWeaponAttachmentSlotItemModel result) {
            return this.slotItems.TryGet(where: static (it, filter) => it.WeaponSlotType == filter.slotType, filter: (slotType, false), out result);
        }

        public GameInventoryWeaponAttachmentSlotItemModel UpdateSlotItems(int frameNum, CharacterLoadoutSlots slotType, WeaponAttachmentSlots weaponSlotType, EntityRef itemEntity) {
            var model = this.slotItems.GetAndRefresh(frameNum, itemEntity,
                where: static (it, filter) => it.SlotType == filter.slotType && it.WeaponSlotType == filter.weaponSlotType,
                filter: (slotType, weaponSlotType));

            model.SlotType       = slotType;
            model.WeaponSlotType = weaponSlotType;
            return model;
        }

        public void DeleteOutdated(int frameNum) {
            this.slotItems.DeleteOutdatedItems(frameNum);
        }
    }

    public class GameInventoryWeaponAttachmentSlotItemModel {
        public CharacterLoadoutSlots SlotType;
        public WeaponAttachmentSlots WeaponSlotType;
        public EntityRef             ItemEntity;
        public int                   Frame;
        
        public MutableAtom<int>  RemainingUsages { get; } = Atom.Value(0);
    }
}