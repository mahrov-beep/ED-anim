namespace Game.Domain.Safe {
    using System.Collections.Generic;
    using GameInventory;
    using Multicast;
    using Quantum;
    using Shared.UserProfile.Data;
    using UniMob;

    public class SafeModel : Model {
        [Inject] private SdUserProfile userProfile;

        private readonly AtomEntityRefList<GameInventoryTrashItemModel> safeItems;

        public SafeModel(Lifetime lifetime) : base(lifetime) {
            this.safeItems = new AtomEntityRefList<GameInventoryTrashItemModel>(lifetime, () => new GameInventoryTrashItemModel(), it => ref it.Frame, it => ref it.ItemEntity);
        }

        [Atom] public int Width        { get; private set; }
        [Atom] public int Height       { get; private set; }
        [Atom] public int UpdatedFrame { get; private set; }

        [Atom] public CellsRange? HighlightedSuccess { get; private set; }
        [Atom] public CellsRange? HighlightedFail    { get; private set; }

        public List<GameInventoryTrashItemModel> EnumerateItems() => this.safeItems.AsList;

        public GameInventoryTrashItemModel UpdateItem(int frameNum, EntityRef itemEntity, int index) {
            return this.safeItems.GetAndRefresh(frameNum, itemEntity, index);
        }

        public void DeleteOutdated(int frameNum) {
            this.safeItems.DeleteOutdatedItems(frameNum);
            this.UpdatedFrame = frameNum;
        }

        public void SetSize(int width, int height) {
            this.Width  = width;
            this.Height = height;
        }

        public bool TryGetItem(EntityRef itemRef, out GameInventoryTrashItemModel result) {
            return this.safeItems.TryGet(where: static (it, filter) => it.ItemEntity == filter.itemRef, filter: (itemRef, false), out result);
        }

        public void SetHighlightedSuccess(CellsRange range) {
            this.HighlightedSuccess = range;
            this.HighlightedFail    = null;
        }

        public void SetHighlightedFail(CellsRange range) {
            this.HighlightedFail    = range;
            this.HighlightedSuccess = null;
        }

        public void ClearHighlighting() {
            this.HighlightedSuccess = null;
            this.HighlightedFail    = null;

        }
    }
}


