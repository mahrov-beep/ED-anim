namespace Game.UI.Widgets.Storage.NearbyItems {
    using System.Collections.Generic;
    using System.Linq;
    using Domain.GameInventory;
    using Domain.ItemBoxStorage;
    using Game;
    using GameInventory;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class NearbyItemsStorageWidget : StorageWidget {
    }

    public class NearbyItemsStorageState : StorageState<NearbyItemsStorageWidget> {
        [Inject] private PhotonService          photonService;
        [Inject] private GameNearbyItemsModel   gameNearbyItemsModel;
        [Inject] private ItemBoxStorageModel    itemBoxStorageModel;

        [Atom] private InventoryItemFilter Filter { get; set; } = InventoryItemFilter.All;

        public override bool ShowTakeAllButton   => true;
        public override bool ShowEquipBestButton => true;

        public NearbyItemsStorageState() {
            this.filtersState = this.CreateChild(x => GameInventoryState.BuildFilters(x, this.Filter, this.OnFilterClick));
        }
        
        protected override Widget BuildItems(BuildContext context) {
            var width = this.itemBoxStorageModel.Width;
            var height = this.itemBoxStorageModel.Height;
            var items = this.itemBoxStorageModel.EnumerateItems();
            
            if (width > 0 && height > 0) {
                return new GameInventoryTetrisWidget {
                    UpdatedFrame          = this.itemBoxStorageModel.UpdatedFrame,
                    NoDraggingInInventory = false,
                    Width                 = width,
                    Height                = height,
                    Items                 = items,
                    Source                = TetrisSource.Storage,
                    InShop                = false,
                    MaxHeight             = 10,
                };
            }

            return new Empty();
        }
        
        private void OnFilterClick(InventoryItemFilter filter) {
            if (this.Filter == filter) {
                return;
            }

            this.Filter = filter;
        }
        
        public override void TakeAll() {
            base.TakeAll();
            
            this.storageApi.DropAllItemsToStorage();
        }

        public override void EquipBest() {
            base.EquipBest();
            
            this.storageApi.EquipBest();
        }

        protected override Widget BuildItem(ItemDescription itemDescription) {
            return new NearbyItemsStorageItemWidget {
                Model = itemDescription.NearbyModel,
                Key   = Key.Of(itemDescription.NearbyModel!),
            };
        }

        protected override IEnumerable<ItemDescription> EnumerateItems() {
            return this.gameNearbyItemsModel.EnumerateNearbyItems()
                .Select(it => new ItemDescription {
                    NearbyModel = it,
                })
                .Where(it => GameInventoryState.FilterItems(this.Filter, this.GetItemAsset(it)));
        }

        protected override ItemAsset GetItemAsset(ItemDescription itemDescription) {
            var itemEntity = itemDescription.NearbyModel!.ItemEntity;
            var item       = this.photonService.PredictedFrame!.Get<Item>(itemEntity);
            var itemAsset  = this.photonService.PredictedFrame!.FindAsset(item.Asset);
            return itemAsset;
        }
    }
}