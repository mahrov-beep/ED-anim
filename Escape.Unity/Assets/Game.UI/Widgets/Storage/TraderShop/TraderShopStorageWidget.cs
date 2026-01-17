namespace Game.UI.Widgets.Storage.TraderShop {
    using System.Collections.Generic;
    using System.Linq;
    using Domain.GameInventory;
    using Domain.ItemBoxStorage;
    using Domain.TraderShop;
    using GameInventory;
    using Multicast;
    using Quantum;
    using Shared.UserProfile.Data;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views;
    using Widgets.TraderShop;

    [RequireFieldsInit]
    public class TraderShopStorageWidget : StorageWidget {
    }

    public class TraderShopStorageState : StorageState<TraderShopStorageWidget> {
        [Inject] private SdUserProfile       userProfile;
        [Inject] private TraderShopApi       traderShopApi;
        [Inject] private TraderShopModel     traderShopModel;
        [Inject] private ItemBoxStorageModel itemBoxStorageModel;

        [Atom] private InventoryItemFilter Filter { get; set; } = InventoryItemFilter.All;

        public TraderShopStorageState() {
            this.filtersState = this.CreateChild(x => GameInventoryState.BuildFilters(x, this.Filter, this.OnFilterClick));
        }
        
        protected override Widget BuildItems(BuildContext context) {
            var width  = this.itemBoxStorageModel.Width;
            var height = this.itemBoxStorageModel.Height;

            var items = this.itemBoxStorageModel.EnumerateItems();
        
            items = items.Where(it => !this.traderShopModel.EnumerateToSellGuids().Contains(it.ItemGuid)).ToList();

            return new GameInventoryTetrisWidget {
                UpdatedFrame          = this.itemBoxStorageModel.UpdatedFrame,
                NoDraggingInInventory = false,
                Width                 = width,
                Height                = height,
                Items                 = items,
                Source                = TetrisSource.Storage,
                InShop                = true,
                MaxHeight             = 10,
            };
        }
        
        private void OnFilterClick(InventoryItemFilter filter) {
            if (this.Filter == filter) {
                return;
            }

            this.Filter = filter;
        }
        
        protected override Widget BuildItem(ItemDescription itemDescription) {
            return new TraderShopStorageItemWidget {
                Item = itemDescription.Item,
                Payload = new DragAndDropPayloadItemFromTraderShopStorage {
                    ItemGuid = itemDescription.Item!.ItemGuid,
                },

                Key = Key.Of(itemDescription.Item.ItemGuid!),
            };
        }

        protected override IEnumerable<ItemDescription> EnumerateItems() {
            return this.userProfile.Storage.Lookup
                .Where(it => !this.traderShopModel.EnumerateToSellGuids().Contains(it.ItemGuid))
                .Select(it => new ItemDescription {
                    Item = it.Item.Value,
                })
                .Where(it => GameInventoryState.FilterItems(this.Filter, this.GetItemAsset(it)));
        }

        protected override ItemAsset GetItemAsset(ItemDescription itemDescription) {
            return QuantumUnityDB.GetGlobalAsset(ItemAssetCreationData.GetItemAssetPath(itemDescription.Item!.ItemKey)) as ItemAsset;
        }

        public override bool CanDropItemToStorage(DragAndDropPayloadItem payload) {
            if (this.traderShopApi.CanMoveItemToStorage(payload)) {
                return true;
            }

            if (payload is DragAndDropPayloadItemFromTraderShopItems) {
                return false;
            }

            return base.CanDropItemToStorage(payload);
        }

        public override void OnDropItemToStorage(DragAndDropPayloadItem payload) {
            if (this.traderShopApi.CanMoveItemToStorage(payload)) {
                this.traderShopApi.MoveItemToStorage(payload);
                return;
            }

            if (payload is DragAndDropPayloadItemFromTraderShopItems) {
                return;
            }

            base.OnDropItemToStorage(payload);
        }
    }
}