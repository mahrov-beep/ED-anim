namespace Game.UI.Widgets.Storage.TraderShop {
    using Controllers.Features.SelectedItemInfo;
    using Domain;
    using Domain.items;
    using Domain.TraderShop;
    using Items;
    using Multicast;
    using Multicast.GameProperties;
    using Multicast.Numerics;
    using Quantum;
    using Shared;
    using Shared.Balance;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using SoundEffects;
    using UniMob;
    using UniMob.UI;
    using Views;

    [RequireFieldsInit]
    public class TraderShopTraderItemWidget : StatefulWidget, IStorageItemWidget {
        public GameSnapshotLoadoutItem Item;
        public DragAndDropPayloadItem  Payload;
    }

    public class TraderShopTraderItemState : StorageItemState<TraderShopTraderItemWidget> {
        [Inject] private GameDef             gameDef;
        [Inject] private ItemsModel          itemsModel;
        [Inject] private SdUserProfile       userProfile;
        [Inject] private TraderShopModel     traderShopModel;
        [Inject] private GamePropertiesModel properties;

        public override float Weight => 0f;

        public override Cost ItemCost => ItemBalance.CalculateBuyCost(this.gameDef, this.Widget.Item);

        protected override ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Widget.Item.ItemKey)
        );

        [Atom] private ItemDef ItemDef => this.gameDef.Items.Get(this.ItemAsset.ItemKey);

        [Atom] private bool HasEnoughTraderLevel => this.userProfile.Threshers.Get(SharedConstants.Game.Threshers.TRADER).Level.Value >= this.ItemDef.MinTraderLevelToBuy
                                                    || this.properties.Get(GameProperties.Booleans.IgnoreTraderShopBlockers);

        public override bool Draggable => this.HasEnoughTraderLevel;

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() {
            return this.Widget.Payload;
        }

        public override void Select() {
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);

            SelectedItemInfoFeatureEvents.Select.Raise(new SelectedItemInfoFeatureEvents.SelectArgs {
                ItemKey             = this.ItemAsset.ItemKey,
                ItemEntity          = EntityRef.None,
                ItemSnapshot        = this.Widget.Item,
                Position            = WidgetPosition.Position.Left,
                IsTakeButtonVisible = false,
            });
        }
        public override void OnDoubleClick() {
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);

            switch (this.Widget.Payload) {
                case DragAndDropPayloadItemFromTraderShopToSell:
                    this.traderShopModel.RemoveToSellGuid(this.Widget.Item.ItemGuid);
                    return;
                case DragAndDropPayloadItemFromTraderShopToBuy:
                    this.traderShopModel.RemoveToBuyGuid(this.Widget.Item.ItemGuid);
                    return;
            }
            
            this.traderShopModel.AddToBuyGuid(this.Widget.Item.ItemGuid);
        }

        protected override Widget BuildDetails(BuildContext context) {
            if (!this.HasEnoughTraderLevel) {
                return new StorageItemBlockerWidget {
                    BlockerText = $"Unlocks at<br><size=130%><b>Trader Lvl {this.ItemDef.MinTraderLevelToBuy}",
                };
            }

            return new SnapshotItemDetailsWidget {
                Item = this.Widget.Item,
            };
        }
    }
}