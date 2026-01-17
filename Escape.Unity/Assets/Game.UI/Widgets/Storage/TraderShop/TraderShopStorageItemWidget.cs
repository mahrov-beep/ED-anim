namespace Game.UI.Widgets.Storage.TraderShop {
    using Controllers.Features.SelectedItemInfo;
    using Domain;
    using Domain.items;
    using Domain.TraderShop;
    using Items;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Numerics;
    using Quantum;
    using Shared;
    using Shared.Balance;
    using Shared.UserProfile.Data;
    using SoundEffects;
    using UniMob.UI;
    using Views;

    [RequireFieldsInit]
    public class TraderShopStorageItemWidget : StatefulWidget, IStorageItemWidget {
        public GameSnapshotLoadoutItem Item;
        public DragAndDropPayloadItem  Payload;
    }

    public class TraderShopStorageItemState : StorageItemState<TraderShopStorageItemWidget> {
        [Inject] private GameDef         gameDef;
        [Inject] private ItemsModel      itemsModel;
        [Inject] private SdUserProfile   userProfile;
        [Inject] private TraderShopModel traderShopModel;

        [CanBeNull] private ItemModel ItemModel => this.itemsModel.TryGet(this.ItemKey, out var itemModel) ? itemModel : null;

        public override float Weight => 0f;

        public override Cost ItemCost => ItemBalance.CalculateSellCost(this.gameDef, this.Widget.Item);

        protected override ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Widget.Item.ItemKey)
        );

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() {
            return this.Widget.Payload;
        }

        public override void Select() {
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);

            SelectedItemInfoFeatureEvents.Select.Raise(new SelectedItemInfoFeatureEvents.SelectArgs {
                ItemKey             = this.ItemAsset.ItemKey,
                ItemEntity          = EntityRef.None,
                ItemSnapshot        = this.Widget.Item,
                Position            = WidgetPosition.Position.Right,
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
            return new SnapshotItemDetailsWidget {
                Item = this.Widget.Item,
            };
        }
    }
}