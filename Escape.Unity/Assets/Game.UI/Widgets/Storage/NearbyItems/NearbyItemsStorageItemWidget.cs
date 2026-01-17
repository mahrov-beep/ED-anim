namespace Game.UI.Widgets.Storage.NearbyItems {
    using Controllers.Features.SelectedItemInfo;
    using Domain;
    using Domain.GameInventory;
    using Domain.TraderShop;
    using Items;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using SoundEffects;
    using UniMob.UI;
    using Views;
    using Views.Storage;

    [RequireFieldsInit]
    public class NearbyItemsStorageItemWidget : StatefulWidget, IStorageItemWidget {
        public GameNearbyItemModel Model;
    }

    public class NearbyItemsStorageItemState : StorageItemState<NearbyItemsStorageItemWidget>, IStorageItemState {
        [Inject] private PhotonService   photonService;
        [Inject] private TraderShopModel traderShopModel;

        private EntityRef ItemEntity => this.Widget.Model.ItemEntity;
        private Item      Item       => this.photonService.PredictedFrame!.Get<Item>(this.ItemEntity);

        protected override ItemAsset ItemAsset => this.photonService.PredictedFrame!.FindAsset(this.Item.Asset);

        public override float Weight => Item.GetItemWeight(this.photonService.PredictedFrame, this.ItemEntity).AsFloat;

        public override DragAndDropPayloadItem GetDragAndDropItemPayload() {
            throw new System.NotImplementedException();
        }
        
        public override void Select() {
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);

            SelectedItemInfoFeatureEvents.Select.Raise(new SelectedItemInfoFeatureEvents.SelectArgs {
                ItemKey             = this.ItemAsset.ItemKey,
                ItemEntity          = this.ItemEntity,
                ItemSnapshot        = null,
                Position            = WidgetPosition.Position.Right,
                IsTakeButtonVisible = true,
            });
        }
        
        public override void OnDoubleClick() {
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);

            this.traderShopModel.AddToBuyGuid(this.Item.MetaGuid);
        }

        protected override Widget BuildDetails(BuildContext context) {
            return new EntityItemDetailsWidget {
                ItemEntity = this.ItemEntity,
            };
        }
    }
}