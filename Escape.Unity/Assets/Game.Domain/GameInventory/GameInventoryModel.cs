namespace Game.Domain.GameInventory {
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Multicast;
    using Quantum;
    using UniMob;

    public class GameInventoryModel : Model {
        private readonly AtomEntityRefList<GameInventorySlotItemModel>  slotItems;
        private readonly AtomEntityRefList<GameInventoryTrashItemModel> trashItems;

        public GameInventoryModel(Lifetime lifetime) : base(lifetime) {
            this.slotItems  = new AtomEntityRefList<GameInventorySlotItemModel>(lifetime, () => new GameInventorySlotItemModel(), it => ref it.Frame, it => ref it.ItemEntity);
            this.trashItems = new AtomEntityRefList<GameInventoryTrashItemModel>(lifetime, () => new GameInventoryTrashItemModel(), it => ref it.Frame, it => ref it.ItemEntity);

            this.PrimaryWeapon   = new GameWeaponModel(lifetime);
            this.SecondaryWeapon = new GameWeaponModel(lifetime);
            this.MeleeWeapon     = new GameWeaponModel(lifetime);

            this.AbilityModel = new GameAbilityModel(lifetime);

            this.PrimaryWeaponAttachments   = new GameInventoryWeaponAttachmentsModel(lifetime);
            this.SecondaryWeaponAttachments = new GameInventoryWeaponAttachmentsModel(lifetime);
            this.MeleeWeaponAttachments     = new GameInventoryWeaponAttachmentsModel(lifetime);
        }

        public GameWeaponModel PrimaryWeapon   { get; }
        public GameWeaponModel SecondaryWeapon { get; }
        public GameWeaponModel MeleeWeapon     { get; }

        public GameAbilityModel AbilityModel { get; }

        public GameInventoryWeaponAttachmentsModel PrimaryWeaponAttachments   { get; }
        public GameInventoryWeaponAttachmentsModel SecondaryWeaponAttachments { get; }
        public GameInventoryWeaponAttachmentsModel MeleeWeaponAttachments     { get; }

        [Atom] public EntityRef SelectedItem { get; private set; }

        public void SetSelectedItem(EntityRef itemRef) => this.SelectedItem = itemRef;

        [Atom] public float CurrentItemWeight { get; set; }
        [Atom] public float LimitItemsWeight  { get; set; }
        [Atom] public int   LoadoutQuality    { get; set; }
        [Atom] public int   LoadoutWidth      { get; set; }
        [Atom] public int   LoadoutHeight     { get; set; }
        [Atom] public bool  IsUsageLocked     { get; set; }
        [Atom] public int   UpdatedFrame      { get; set; }

        public CellsRange? HighlightedSuccess { get; set; }
        public CellsRange? HighlightedFail    { get; set; }

        public CharacterLoadoutSlots[] AllValidSlots => CharacterLoadoutSlotsExtension.AllValidSlots;

        [CanBeNull] public GameWeaponModel GetWeaponModel(CharacterLoadoutSlots slot) => slot switch {
            CharacterLoadoutSlots.PrimaryWeapon => this.PrimaryWeapon,
            CharacterLoadoutSlots.SecondaryWeapon => this.SecondaryWeapon,
            CharacterLoadoutSlots.MeleeWeapon => this.MeleeWeapon,
            _ => null,
        };

        [CanBeNull] public GameInventoryWeaponAttachmentsModel GetWeaponAttachmentsModel(CharacterLoadoutSlots slot) => slot switch {
            CharacterLoadoutSlots.PrimaryWeapon => this.PrimaryWeaponAttachments,
            CharacterLoadoutSlots.SecondaryWeapon => this.SecondaryWeaponAttachments,
            CharacterLoadoutSlots.MeleeWeapon => this.MeleeWeaponAttachments,
            _ => null,
        };

        public bool TryGetSlotItem(CharacterLoadoutSlots slotType, out GameInventorySlotItemModel result) {
            return this.slotItems.TryGet(where: static (it, filter) => it.SlotType == filter.slotType, filter: (slotType, false), out result);
        }

        public bool TryGetTrashItem(EntityRef itemRef, out GameInventoryTrashItemModel result) {
            return this.trashItems.TryGet(where: static (it, filter) => it.ItemEntity == filter.itemRef, filter: (itemRef, false), out result);
        }

        public List<GameInventoryTrashItemModel> EnumerateTrashItems() {
            return this.trashItems.AsList;
        }

        public GameInventorySlotItemModel UpdateSlotItems(int frameNum, CharacterLoadoutSlots slotType, EntityRef itemEntity) {
            var model = this.slotItems.GetAndRefresh(frameNum, itemEntity, where: static (it, f) => it.SlotType == f.slotType, (slotType, itemEntity));
            model.SlotType = slotType;
            return model;
        }

        public GameInventoryTrashItemModel UpdateTrashItem(int frameNum, EntityRef itemEntity, int indexInTrash) {
            return this.trashItems.GetAndRefresh(frameNum, itemEntity, indexInTrash);
        }

        public void DeleteOutdated(int frameNum) {
            this.slotItems.DeleteOutdatedItems(frameNum);
            this.trashItems.DeleteOutdatedItems(frameNum);

            this.PrimaryWeaponAttachments.DeleteOutdated(frameNum);
            this.SecondaryWeaponAttachments.DeleteOutdated(frameNum);
            this.MeleeWeaponAttachments.DeleteOutdated(frameNum);
        }
    }

    public class GameInventorySlotItemModel {
        public CharacterLoadoutSlots SlotType;
        public EntityRef             ItemEntity;
        public int                   Frame;

        public MutableAtom<bool> IsBlocked       { get; } = Atom.Value(false);
        public MutableAtom<int>  RemainingUsages { get; } = Atom.Value(0);
    }

    public class GameWeaponModel : ILifetimeScope {
        public GameWeaponModel(Lifetime lifetime) {
            this.Lifetime = lifetime;
        }

        [Atom] public int Bullets         { get; set; }
        [Atom] public int MaxBullets      { get; set; }
        [Atom] public int AmmoInInventory { get; set; }

        [Atom] public bool IsSelected { get; set; }

        [Atom] public bool  IsWeaponChanging    { get; set; }
        [Atom] public float ReloadingTimer      { get; set; }
        [Atom] public float WeaponChangingTimer { get; set; }

        public Lifetime Lifetime { get; set; }
    }

    public class GameAbilityModel : ILifetimeScope {
        public GameAbilityModel(Lifetime lifetime) {
            Lifetime = lifetime;
        }

        [Atom] public bool  HasAbility     { get; set; }
        [Atom] public float ReloadingTimer { get; set; }

        public Lifetime Lifetime { get; set; }
    }

    public class GameInventoryTrashItemModel {
        public EntityRef ItemEntity;
        public ItemAsset ItemAsset;
        public int       Frame;
        public QGuid     ItemGuid;

        public MutableAtom<int>   IndexI          { get; } = Atom.Value(0);
        public MutableAtom<int>   IndexJ          { get; } = Atom.Value(0);
        public MutableAtom<bool>  CanBeUsed       { get; } = Atom.Value(false);
        public MutableAtom<int>   RemainingUsages { get; } = Atom.Value(0);
        public MutableAtom<bool>  Rotated         { get; } = Atom.Value(false);
        public MutableAtom<float> Weight          { get; } = Atom.Value(0f);
        public MutableAtom<bool>  IsFromSafe      { get; } = Atom.Value(false);
    }
}