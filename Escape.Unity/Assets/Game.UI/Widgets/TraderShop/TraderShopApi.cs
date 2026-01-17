namespace Game.UI.Widgets.TraderShop {
    using Domain.TraderShop;
    using Multicast;
    using UnityEngine;
    using Views;

    public class TraderShopApi {
        [Inject] private TraderShopModel traderShopModel;


        public bool CanMoveItemToStorage(DragAndDropPayloadItem payload) {
            return payload is DragAndDropPayloadItemFromTraderShopToSell;
        }

        public void MoveItemToStorage(DragAndDropPayloadItem payload) {
            switch (payload) {
                case DragAndDropPayloadItemFromTraderShopToSell toSell:
                    this.traderShopModel.RemoveToSellGuid(toSell.ItemGuid);
                    break;

                default:
                    Debug.LogError($"Cannot move item to storage from source={payload?.GetType()}");
                    break;
            }
        }

        public bool CanMoveItemToSell(DragAndDropPayloadItem payload) {
            return payload is DragAndDropPayloadItemEntityFromTetris;
        }

        public void MoveItemToSell(DragAndDropPayloadItem payload) {
            switch (payload) {
                case DragAndDropPayloadItemEntityFromTetris fromTetris:
                    this.traderShopModel.AddToSellGuid(fromTetris.ItemGuid);
                    break;

                default:
                    Debug.LogError($"Cannot move item to sell from source={payload?.GetType()}");
                    break;
            }
        }

        public bool CanMoveItemToBuy(DragAndDropPayloadItem payload) {
            return payload is DragAndDropPayloadItemFromTraderShopItems;
        }

        public void MoveItemToBuy(DragAndDropPayloadItem payload) {
            switch (payload) {
                case DragAndDropPayloadItemFromTraderShopItems fromTraderItems:
                    this.traderShopModel.AddToBuyGuid(fromTraderItems.ItemGuid);
                    break;

                default:
                    Debug.LogError($"Cannot move item to buy from source={payload?.GetType()}");
                    break;
            }
        }

        public bool CanMoveItemToTrader(DragAndDropPayloadItem payload) {
            return payload is DragAndDropPayloadItemFromTraderShopToBuy;
        }

        public void MoveItemToTrader(DragAndDropPayloadItem payload) {
            switch (payload) {
                case DragAndDropPayloadItemFromTraderShopToBuy fromTraderShopToBuy:
                    this.traderShopModel.RemoveToBuyGuid(fromTraderShopToBuy.ItemGuid);
                    break;

                default:
                    Debug.LogError($"Cannot move item to buy from source={payload?.GetType()}");
                    break;
            }
        }
    }
}