namespace Game.UI.Widgets.GameInventory {
    using System.Collections.Generic;
    using System.Linq;
    using Domain.GameInventory;
    using Domain.ItemBoxStorage;
    using Domain.Safe;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views;
    using Views.GameInventory;

    [RequireFieldsInit]
    public class GameInventoryTetrisWidget : StatefulWidget {
        public int UpdatedFrame;

        public bool NoDraggingInInventory;

        public int Width;

        public int Height;

        public List<GameInventoryTrashItemModel> Items;
        
        public TetrisSource Source;

        public bool InShop;

        public int MaxHeight;
    }

    public class GameInventoryTetrisState : ViewState<GameInventoryTetrisWidget>, IGameInventoryTetrisState {
        [Inject] private GameInventoryModel  gameInventoryModel;
        [Inject] private PhotonService       photonService;
        [Inject] private GameInventoryApi    gameInventoryApi;
        [Inject] private SafeModel           safeModel;
        [Inject] private ItemBoxStorageModel itemBoxStorageModel;

        private List<GameInventoryCellWidget> cellWidgets;
        private readonly StateHolder tetris;
        private readonly StateHolder cells;

        public override WidgetViewReference View => UiConstants.Views.GameInventory.Tetris;

        public IState Tetris => this.tetris.Value;
        public IState Cells  => this.cells.Value;

        public GameInventoryTetrisState() {
            this.cells  = this.CreateChild(this.BuildCells);
            this.tetris = this.CreateChild(this.BuildTetris);
        }

        public override WidgetSize CalculateSize() {
            return WidgetSize.StackZ(this.cells.Value.Size, this.tetris.Value.Size);
        }

        private Widget BuildCells(BuildContext context) {
            if (this.Widget.Height > this.Widget.MaxHeight) {
                return new Empty();
            }
            
            this.cellWidgets = new List<GameInventoryCellWidget>();
            
            for (var i = 0; i < this.Widget.Height; i++) {
                for (var j = 0; j < this.Widget.Width; j++) {
                    this.cellWidgets.Add(this.BuildCell(i, j));
                }
            }

            return new GridFlow {
                MainAxisAlignment  = MainAxisAlignment.Start,
                CrossAxisAlignment = CrossAxisAlignment.Center,
                MaxCrossAxisCount  = this.Widget.Width,
                ChildrenBuilder = () => this.cellWidgets
                    .Select(x => (Widget)x)
                    .ToList(),
            };
        }

        private Widget BuildTetris(BuildContext context) {
            var backgrounds = new List<Widget>();
            var items       = new List<Widget>();

            for (var i = 0; i < this.Widget.Height; i++) {
                for (var j = 0; j < this.Widget.Width; j++) {
                    backgrounds.Add(this.BuildCell(i, j));
                    items.Add(this.BuildCellWithItem(i, j));
                }
            }

            if (this.Widget.Height > this.Widget.MaxHeight) {
                return new ScrollGridFlowWithBg {
                    BackgroundBuilder = () => backgrounds,
                    ChildrenBuilder = () => items,
                    MainAxisAlignment = MainAxisAlignment.Start,
                    CrossAxisAlignment = CrossAxisAlignment.Center,
                    MaxCrossAxisCount = this.Widget.Width,
                };
            }

            return new ZStack {
                Children = {
                    new GridFlow {
                        MainAxisAlignment = MainAxisAlignment.Start,
                        CrossAxisAlignment = CrossAxisAlignment.Center,
                        MaxCrossAxisCount = this.Widget.Width,
                        ChildrenBuilder = () => backgrounds,
                    },
                    new GridFlow {
                        MainAxisAlignment = MainAxisAlignment.Start,
                        CrossAxisAlignment = CrossAxisAlignment.Center,
                        MaxCrossAxisCount = this.Widget.Width,
                        ChildrenBuilder = () => items,
                    },
                },
            };
        }
        private GameInventoryCellWidget BuildCell(int i, int j) {
            return new GameInventoryCellWidget {
                View                  = UiConstants.Views.GameInventory.Cell,
                Item                  = default,
                NoDraggingInInventory = this.Widget.NoDraggingInInventory,
                IndexI                = i,
                IndexJ                = j,
                OnCellDrag            = this.OnCellDrag,
                Source                = this.Widget.Source,
                InShop                = this.Widget.InShop,
            };
        }

        private void OnCellDrag(DragAndDropPayloadItem payload, CellsRange dropRange, bool succeed) {
            var entity = payload switch {
                DragAndDropPayloadItemEntityFromTetris fromTetris => fromTetris.ItemEntity,
                DragAndDropPayloadItemEntityFromSlot fromSlot => fromSlot.ItemEntity,
                DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot => fromWeaponSlot.ItemEntity,
                _ => EntityRef.None,
            };

            this.OnCellDrag(entity, dropRange, succeed);
        }

        private void OnCellDrag(EntityRef entityRef, CellsRange dropRange, bool succeed) {
            if (dropRange.Width == 0 || dropRange.Height == 0) {
                this.ClearHighlighting();
                return;
            }

            if (!succeed) {
                this.SetHighlightedFail(dropRange);
                return;
            }

            this.SetHighlightedSuccess(dropRange);
        }

        private void ClearHighlighting() {
            if (this.Widget.Source == TetrisSource.Safe) {
                this.safeModel.ClearHighlighting();
            } 
            else if (this.Widget.Source == TetrisSource.Storage) {
                this.itemBoxStorageModel.ClearHighlighting();
            }
            else {
                this.gameInventoryModel.HighlightedFail    = null;
                this.gameInventoryModel.HighlightedSuccess = null;
            }
        }

        private void SetHighlightedSuccess(CellsRange range) {
            if (this.Widget.Source == TetrisSource.Safe) {
                this.safeModel.SetHighlightedSuccess(range);
            } 
            else if (this.Widget.Source == TetrisSource.Storage) {
                this.itemBoxStorageModel.SetHighlightedSuccess(range);
            }
            else {
                this.gameInventoryModel.HighlightedFail    = null;
                this.gameInventoryModel.HighlightedSuccess = range;
            }
        }

        private void SetHighlightedFail(CellsRange range) {
            if (this.Widget.Source == TetrisSource.Safe) {
                this.safeModel.SetHighlightedFail(range);
            } 
            else if (this.Widget.Source == TetrisSource.Storage) {
                this.itemBoxStorageModel.SetHighlightedFail(range);
            }
            else {
                this.gameInventoryModel.HighlightedSuccess = null;
                this.gameInventoryModel.HighlightedFail    = range;
            }
        }

        private bool FirstItemWithCoordinates(EntityRef entityRef, int i, int j) {
            GameInventoryTrashItemModel itemModel;

            if (this.Widget.Source == TetrisSource.Safe) {
                if (!this.safeModel.TryGetItem(entityRef, out itemModel)) {
                    return false;
                }
            } 
            else if (this.Widget.Source == TetrisSource.Storage) {
                if (!this.itemBoxStorageModel.TryGetItem(entityRef, out itemModel)) {
                    return false;
                }
            }
            else {
                if (!this.gameInventoryModel.TryGetTrashItem(entityRef, out itemModel)) {
                    return false;
                }
            }

            return itemModel.IndexI.Value == i && itemModel.IndexJ.Value == j;
        }

        private GameInventoryCellWidget BuildCellWithItem(int i, int j) {
            return new GameInventoryCellWidget {
                View                  = UiConstants.Views.GameInventory.CellWithItem,
                Item                  = this.Widget.Items.FirstOrDefault(x => this.FirstItemWithCoordinates(x.ItemEntity, i, j)),
                NoDraggingInInventory = this.Widget.NoDraggingInInventory,
                IndexI                = i,
                IndexJ                = j,
                OnCellDrag            = this.OnCellDrag,
                Source                = this.Widget.Source,
                InShop                = this.Widget.InShop,
            };
        }
    }
}