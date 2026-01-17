namespace Game.UI.Widgets.TraderShop {
    using System;
    using System.Linq;
    using Controllers.Features.TraderShop;
    using Domain;
    using Domain.GameInventory;
    using Domain.ItemBoxStorage;
    using Domain.items;
    using Domain.Storage;
    using Domain.TraderShop;
    using GameInventory;
    using Items;
    using Multicast;
    using Multicast.Numerics;
    using Quantum;
    using Shared.UserProfile.Data;
    using SoundEffects;
    using Storage.TraderShop;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views;
    using Views.TraderShop;

    [RequireFieldsInit]
    public class TraderShopWidget : StatefulWidget {
        public Action OnClose;
    }

    public class TraderShopState : ViewState<TraderShopWidget>, ITraderShopState {
        private const float ITEM_WIDTH = 160f;

        [Inject] private ItemsModel          itemsModel;
        [Inject] private SdUserProfile       userProfile;
        [Inject] private TraderShopModel     traderShopModel;
        [Inject] private TraderShopApi       traderShopApi;
        [Inject] private StorageModel        storageModel;
        [Inject] private ItemBoxStorageModel itemBoxStorageModel;

        private readonly StateHolder toSellItemsState;
        private readonly StateHolder toBuyItemsState;
        private readonly StateHolder traderItemsState;
        private readonly StateHolder filtersState;

        // private readonly StateHolder<ThresherState> thresherState;

        [Atom] private InventoryItemFilter Filter { get; set; } = InventoryItemFilter.All;

        public TraderShopState() {
            this.toSellItemsState = this.CreateChild(this.BuildToSellItems);
            this.toBuyItemsState  = this.CreateChild(this.BuildToBuyItems);
            this.traderItemsState = this.CreateChild(this.BuildTraderItems);

            this.filtersState = this.CreateChild(x => GameInventoryState.BuildFilters(x, this.Filter, this.OnFilterClick));

            // this.thresherState = this.CreateChild<ThresherWidget, ThresherState>(_ => new ThresherWidget {
            //     ThresherKey = SharedConstants.Game.Threshers.TRADER,
            // });
        }
        
        private void OnFilterClick(InventoryItemFilter filter) {
            if (this.Filter == filter) {
                return;
            }

            this.Filter = filter;
        }

        public override WidgetViewReference View => UiConstants.Views.TraderShop.Screen;

        // public IThresherState Thresher => this.thresherState.Value;

        public IState ToSellItems   => this.toSellItemsState.Value;
        public IState ToBuyItems    => this.toBuyItemsState.Value;
        public IState TraderItems   => this.traderItemsState.Value;
        public IState Filters       => this.filtersState.Value;

        public bool DealAvailable => this.traderShopModel.IsDealAvailable;
        
        [Atom]
        public bool HasEnoughSpaceInStorage {
            get {
                var itemsToBuy = this.traderShopModel.EnumerateToBuyGuids()
                    .Select(it => this.userProfile.TraderShop.Value.TradedItems.Find(a => a.ItemGuid == it)).ToArray();
                
                var itemsToSell = this.traderShopModel.EnumerateToSellGuids().ToHashSet();
                
                return this.storageModel.CanAddItems(
                    itemsToBuy.Where(it => it != null).Select(it => it.ItemKey).ToArray(),
                    itemsToSell);
            }
        }

        public Cost SellCost => this.traderShopModel.SellCost;
        public Cost BuyCost  => this.traderShopModel.BuyCost;

        private Widget BuildToSellItems(BuildContext context) {
            return new ScrollGridFlow {
                Padding            = new RectPadding(0, 0, 20, 100),
                MainAxisAlignment  = MainAxisAlignment.Start,
                CrossAxisAlignment = CrossAxisAlignment.Start,
                MaxCrossAxisExtent = (ITEM_WIDTH * 2) + 10,
                ChildrenBuilder = () => this.traderShopModel.EnumerateToSellGuids()
                    .Where(it => this.userProfile.Storage.Lookup.TryGetValue(it, out var item))
                    .Select(it => this.userProfile.Storage.Get(it))
                    .Select(it => this.BuildToSellItem(it.Item.Value))
                    .Reverse()
                    .ToList(),
            };
        }

        private Widget BuildToSellItem(GameSnapshotLoadoutItem item) {
            var itemEntity = this.itemBoxStorageModel.EnumerateItems().FirstOrDefault(it => it.ItemGuid == item.ItemGuid)!.ItemEntity;
            
            return new TraderShopStorageItemWidget {
                Item = item,
                Payload = new DragAndDropPayloadItemFromTraderShopToSell {
                    ItemEntity = itemEntity,
                    ItemGuid   = item.ItemGuid,
                },

                Key = Key.Of(item.ItemGuid!),
            };
        }

        private Widget BuildToBuyItems(BuildContext context) {
            return new ScrollGridFlow {
                Padding            = new RectPadding(0, 0, 20, 100),
                MainAxisAlignment  = MainAxisAlignment.Start,
                CrossAxisAlignment = CrossAxisAlignment.Start,
                MaxCrossAxisExtent = (ITEM_WIDTH * 2) + 10,
                ChildrenBuilder = () => this.traderShopModel.EnumerateToBuyGuids()
                    .Select(it => this.userProfile.TraderShop.Value.TradedItems.Find(a => a.ItemGuid == it))
                    .Select(it => this.BuildToBuyItem(it))
                    .Reverse()
                    .ToList(),
            };
        }

        private Widget BuildToBuyItem(GameSnapshotLoadoutItem item) {
            if (item == null) {
                return new Empty();
            }
            
            return new TraderShopTraderItemWidget {
                Item = item,
                Payload = new DragAndDropPayloadItemFromTraderShopToBuy {
                    ItemGuid = item.ItemGuid,
                },

                Key = Key.Of(item.ItemGuid!),
            };
        }

        private Widget BuildTraderItems(BuildContext context) {
            return new ScrollGridFlow {
                Padding            = new RectPadding(0, 0, 20, 300),
                MainAxisAlignment  = MainAxisAlignment.Start,
                CrossAxisAlignment = CrossAxisAlignment.Start,
                MaxCrossAxisExtent = (ITEM_WIDTH * 4) + 10,

                ChildrenBuilder = () => this.userProfile.TraderShop.Value.TradedItems
                    .Where(it => !this.traderShopModel.EnumerateToBuyGuids().Contains(it.ItemGuid))
                    .Where(it => GameInventoryState.FilterItems(this.Filter, this.GetItemAsset(it)))
                    .Select(it => (description: it, asset: this.GetItemAsset(it)))
                    .OrderByDescending(it => it.asset.Grouping)
                    .ThenByDescending(it => it.asset.rarity)
                    .ThenBy(it => it.asset.ItemKey)
                    .GroupBy(it => it.asset.Grouping)
                    .SelectMany(it => Enumerable.Empty<Widget>()
                        .Prepend(this.BuildTraderItemsSeparator(it.Key))
                        .Concat(it.Select(a => this.BuildTraderItem(a.description))))
                    .ToList(),
            };
        }

        private Widget BuildTraderItemsSeparator(ItemAssetGrouping grouping) {
            return new ItemGroupingSeparatorWidget {
                Grouping = grouping,
                Width    = ITEM_WIDTH * 4,
            };
        }


        private Widget BuildTraderItem(GameSnapshotLoadoutItem item) {
            return new TraderShopTraderItemWidget {
                Item = item,
                Payload = new DragAndDropPayloadItemFromTraderShopItems {
                    ItemGuid = item.ItemGuid,
                },

                Key = Key.Of(item.ItemGuid!),
            };
        }

        public bool CanMoveItemToSell(DragAndDropPayloadItem payload) {
            return this.traderShopApi.CanMoveItemToSell(payload);
        }

        public void OnMoveItemToSell(DragAndDropPayloadItem payload) {
            this.traderShopApi.MoveItemToSell(payload);
            
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);
        }

        public bool CanMoveItemToBuy(DragAndDropPayloadItem payload) {
            return this.traderShopApi.CanMoveItemToBuy(payload);
        }

        public void OnMoveItemToBuy(DragAndDropPayloadItem payload) {
            this.traderShopApi.MoveItemToBuy(payload);
            
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);
        }

        public bool CanMoveItemToTrader(DragAndDropPayloadItem payload) {
            return this.traderShopApi.CanMoveItemToTrader(payload);
        }

        public void OnMoveItemToTrader(DragAndDropPayloadItem payload) {
            this.traderShopApi.MoveItemToTrader(payload);
            
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);
        }

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        public void Deal() {
            TraderShopFeatureEvents.Sell.Raise();
        }

        protected ItemAsset GetItemAsset(GameSnapshotLoadoutItem item) {
            return QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
                ItemAssetCreationData.GetItemAssetPath(item.ItemKey)
            );
        }
    }
}