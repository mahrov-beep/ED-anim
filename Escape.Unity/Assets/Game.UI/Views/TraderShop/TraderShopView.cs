namespace Game.UI.Views.TraderShop {
    using UniMob.UI;
    using Multicast;
    using Multicast.Numerics;
    using Shared;
    using Sirenix.OdinInspector;
    using Threshers;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class TraderShopView : AutoView<ITraderShopState> {
        [SerializeField, Required] private ViewPanel toSellItemsPanel;
        [SerializeField, Required] private ViewPanel toBuyItemsPanel;
        [SerializeField, Required] private ViewPanel traderItemsPanel;
        [SerializeField, Required] private ViewPanel filtersPanel;

        [SerializeField, Required] private UniMobDropZoneBehaviour toSellDropZone;
        [SerializeField, Required] private UniMobDropZoneBehaviour toBuyDropZone;
        [SerializeField, Required] private UniMobDropZoneBehaviour traderItemsDropZone;

        // [SerializeField, Required] private ThresherView thresherView;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("deal_available", () => this.State.DealAvailable, true),
            this.Variable("has_enough_space_in_storage", () => this.State.HasEnoughSpaceInStorage, true),
            this.Variable("sell_cost", () => this.State.SellCost, Cost.Create(cost => {
                cost.Add(SharedConstants.Game.Currencies.BADGES, 999);
                cost.Add(SharedConstants.Game.Currencies.CRYPT, 90);
            })),
            this.Variable("buy_cost", () => this.State.BuyCost, Cost.Create(cost => {
                cost.Add(SharedConstants.Game.Currencies.BADGES, 999);
                cost.Add(SharedConstants.Game.Currencies.CRYPT, 90);
            })),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
            this.Event("deal", () => this.State.Deal()),
        };

        protected override void Awake() {
            base.Awake();

            this.toSellDropZone.IsPayloadAcceptableDelegate = p => {
                if (!this.HasState) {
                    return false;
                }

                return p is DragAndDropPayloadItem itemEntity && this.State.CanMoveItemToSell(itemEntity);
            };
            this.toSellDropZone.OnAccept.AddListener(p => {
                if (this.HasState && p is DragAndDropPayloadItem payloadItem) {
                    this.State.OnMoveItemToSell(payloadItem);
                }
            });

            this.toBuyDropZone.IsPayloadAcceptableDelegate = p => {
                if (!this.HasState) {
                    return false;
                }

                return p is DragAndDropPayloadItem itemEntity && this.State.CanMoveItemToBuy(itemEntity);
            };
            this.toBuyDropZone.OnAccept.AddListener(p => {
                if (this.HasState && p is DragAndDropPayloadItem payloadItem) {
                    this.State.OnMoveItemToBuy(payloadItem);
                }
            });

            this.traderItemsDropZone.IsPayloadAcceptableDelegate = p => {
                if (!this.HasState) {
                    return false;
                }

                return p is DragAndDropPayloadItem itemEntity && this.State.CanMoveItemToTrader(itemEntity);
            };
            this.traderItemsDropZone.OnAccept.AddListener(p => {
                if (this.HasState && p is DragAndDropPayloadItem payloadItem) {
                    this.State.OnMoveItemToTrader(payloadItem);
                }
            });
        }

        protected override void Render() {
            base.Render();

            this.toSellItemsPanel.Render(this.State.ToSellItems, link: true);
            this.toBuyItemsPanel.Render(this.State.ToBuyItems, link: true);
            this.traderItemsPanel.Render(this.State.TraderItems, link: true);
            this.filtersPanel.Render(this.State.Filters, link: true);
            
            // this.thresherView.Render(this.State.Thresher);
        }
    }

    public interface ITraderShopState : IViewState {
        // IThresherState Thresher { get; }

        IState ToSellItems { get; }
        IState ToBuyItems  { get; }
        IState TraderItems { get; }
        IState Filters     { get; }

        bool DealAvailable { get; }

        bool HasEnoughSpaceInStorage { get; }

        Cost SellCost { get; }
        Cost BuyCost  { get; }

        bool CanMoveItemToSell(DragAndDropPayloadItem payload);
        void OnMoveItemToSell(DragAndDropPayloadItem payload);

        bool CanMoveItemToBuy(DragAndDropPayloadItem payload);
        void OnMoveItemToBuy(DragAndDropPayloadItem payload);

        bool CanMoveItemToTrader(DragAndDropPayloadItem payload);
        void OnMoveItemToTrader(DragAndDropPayloadItem payload);

        void Close();
        void Deal();
    }
}