namespace Game.UI.Widgets.GameInventory {
    using System;
    using System.Linq;
    using Domain;
    using Domain.GameInventory;
    using Domain.ItemBoxStorage;
    using Domain.Safe;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using SoundEffects;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Views;
    using Views.GameInventory;

    [RequireFieldsInit]
    public class GameInventoryCellWidget : StatefulWidget {
        public int IndexI;
        public int IndexJ;

        public GameInventoryTrashItemModel Item;
        
        public bool NoDraggingInInventory;

        public Action<DragAndDropPayloadItem, CellsRange, bool> OnCellDrag;

        public WidgetViewReference View;
        
        public TetrisSource Source;

        public bool InShop;
    }

    public class GameInventoryCellState : ViewState<GameInventoryCellWidget>, IGameInventoryCellState {
        [Inject] private GameInventoryModel  gameInventoryModel;
        [Inject] private PhotonService       photonService;
        [Inject] private GameInventoryApi    gameInventoryApi;
        [Inject] private SafeModel           safeModel;
        [Inject] private ItemBoxStorageModel itemBoxStorageModel;

        private readonly StateHolder trashItemState;

        public override WidgetViewReference View => this.Widget.View;

        public IState TrashItem => this.trashItemState.Value;

        public GameInventoryCellState() {
            this.trashItemState = this.CreateChild(this.BuildTrashItems);
        }

        private Widget BuildTrashItems(BuildContext context) {
            if (this.Widget.Item == default) {
                return new Empty();
            }
            
            return this.BuildTrashItem(this.Widget.Item);
        }
        
        private Widget BuildTrashItem(GameInventoryTrashItemModel trashItemModel) {
            if (this.photonService.PredictedFrame is { } f && f.Exists(trashItemModel.ItemEntity)) {
                var it = f.Get<Item>(trashItemModel.ItemEntity);
                
                if (it.SafeGuid.ByteCount != 0 && this.Widget.Source == TetrisSource.Inventory) {
                    return new Empty();
                }
                
                if (it.SafeGuid.ByteCount == 0 && this.Widget.Source == TetrisSource.Safe) {
                    return new Empty();
                }
            }

            return new GameInventoryTrashItemWidget {
                Key         = Key.Of(trashItemModel),
                Model       = trashItemModel,
                IndexI      = this.Widget.IndexI,
                IndexJ      = this.Widget.IndexJ,
                IsHudButton = false,
                NoDragging  = this.Widget.NoDraggingInInventory,
                Source      = this.Widget.Source,
                InShop      = this.Widget.InShop,
            };
        }

        public CellsRange GetDropPlace(DragAndDropPayloadItem payload, out bool valid) {
            return DropPlaceHelper.Compute(
                tryTetrisAt: this.gameInventoryApi.IsEnoughSpaceTetrisAt,
                tryMergeAt: this.gameInventoryApi.CanMergeInto,
                getMetrics: this.gameInventoryApi.GetMetrics,
                payload: payload,
                i: this.Widget.IndexI,
                j: this.Widget.IndexJ,
                valid: out valid,
                source: (byte)this.Widget.Source
            );
        }

        public void OnMoveItemToTrash(DragAndDropPayloadItem payload, CellsRange dropRange) {
            this.gameInventoryApi.MoveItemToTrash(payload, dropRange.I, dropRange.J, dropRange.Rotated, (byte)this.Widget.Source);
            
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);
        }

        public void OnCellDrag(DragAndDropPayloadItem payload, CellsRange dropRange, bool succeed) {
            this.Widget.OnCellDrag(payload, dropRange, succeed);
        }

        public bool IsBusy => this.gameInventoryApi.IsBusyAt(this.Widget.IndexI, this.Widget.IndexJ);
        
        public bool CanAccept {
            get {
                if (this.Widget.Source == TetrisSource.Safe) {
                    return this.safeModel.HighlightedSuccess is { } safeRange && safeRange.Contains(this.Widget.IndexI, this.Widget.IndexJ);
                }
                
                if (this.Widget.Source == TetrisSource.Storage) {
                    return this.itemBoxStorageModel.HighlightedSuccess is { } storageRange && storageRange.Contains(this.Widget.IndexI, this.Widget.IndexJ);
                }
                
                return this.gameInventoryModel.HighlightedSuccess is { } range && range.Contains(this.Widget.IndexI, this.Widget.IndexJ);
            }
        }
        
        public bool CanNotAccept {
            get {
                if (this.Widget.Source == TetrisSource.Safe) {
                    return this.safeModel.HighlightedFail is { } safeRange && safeRange.Contains(this.Widget.IndexI, this.Widget.IndexJ);
                }
                
                if (this.Widget.Source == TetrisSource.Storage) {
                    return this.itemBoxStorageModel.HighlightedFail is { } storageRange && storageRange.Contains(this.Widget.IndexI, this.Widget.IndexJ);
                }
                
                return this.gameInventoryModel.HighlightedFail is { } range && range.Contains(this.Widget.IndexI, this.Widget.IndexJ);
            }
        }

        public int IndexI => this.Widget.IndexI;
        public int IndexJ => this.Widget.IndexJ;
    }
}