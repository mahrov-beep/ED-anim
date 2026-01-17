namespace Game.Domain.ItemBoxStorage {
    using System.Collections.Generic;
    using GameInventory;
    using Multicast;
    using Quantum;
    using UniMob;

    /// <summary>
    /// Модель для отображения ItemBox с тетрис-сеткой
    /// </summary>
    public class ItemBoxStorageModel : Model {
        private readonly AtomEntityRefList<GameInventoryTrashItemModel> items;

        public ItemBoxStorageModel(Lifetime lifetime) : base(lifetime) {
            this.items = new AtomEntityRefList<GameInventoryTrashItemModel>(lifetime,
                () => new GameInventoryTrashItemModel(),
                it => ref it.Frame,
                it => ref it.ItemEntity);
        }

        [Atom] public int Width { get; set; }
        [Atom] public int Height { get; set; }
        [Atom] public int UpdatedFrame { get; set; }
        [Atom] public CellsRange? HighlightedSuccess { get; set; }
        [Atom] public CellsRange? HighlightedFail { get; set; }

        public List<GameInventoryTrashItemModel> EnumerateItems() {
            return this.items.AsList;
        }

        public GameInventoryTrashItemModel UpdateItem(int frameNum, EntityRef itemEntity, int index) {
            return this.items.GetAndRefresh(frameNum, itemEntity, index);
        }

        public bool TryGetItem(EntityRef itemEntity, out GameInventoryTrashItemModel itemModel) {
            return this.items.TryGet(where: static (it, filter) => it.ItemEntity == filter.itemEntity, filter: (itemEntity, false), out itemModel);
        }

        public void DeleteOutdated(int frameNum) {
            this.items.DeleteOutdatedItems(frameNum);
            this.UpdatedFrame = frameNum;
        }

        public void SetSize(int width, int height) {
            this.Width = width;
            this.Height = height;
        }

        public void ClearHighlighting() {
            this.HighlightedSuccess = null;
            this.HighlightedFail = null;
        }

        public void SetHighlightedSuccess(CellsRange range) {
            this.HighlightedFail = null;
            this.HighlightedSuccess = range;
        }

        public void SetHighlightedFail(CellsRange range) {
            this.HighlightedSuccess = null;
            this.HighlightedFail = range;
        }
    }
}

