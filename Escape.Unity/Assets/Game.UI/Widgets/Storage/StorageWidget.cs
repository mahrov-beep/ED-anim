namespace Game.UI.Widgets.Storage {
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Domain.GameInventory;
    using GameInventory;
    using Header;
    using Items;
    using JetBrains.Annotations;
    using Multicast;
    using Quantum;
    using Shared;
    using SoundEffects;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views;
    using Views.Storage;

    [RequireFieldsInit]
    public abstract class StorageWidget : StatefulWidget {
    }

    public abstract class StorageState<TWidget> : ViewState<TWidget>, IStorageState where TWidget : StorageWidget {
        private const float ITEM_WIDTH = 160f;

        [Inject] protected StorageApi storageApi;
        
        private readonly   StateHolder itemsState;
        private readonly   StateHolder headerState;
        
        protected StateHolder filtersState;

        protected StorageState() {
            this.itemsState   = this.CreateChild(this.BuildItems);
            this.headerState  = this.CreateChild(this.BuildHeader);
        }

        public override WidgetViewReference View => UiConstants.Views.Storage.Screen;

        public IState Items   => this.itemsState.Value;
        public IState Header  => this.headerState.Value;
        public IState Filters => this.filtersState.Value;

        public virtual bool CanDropItemToStorage(DragAndDropPayloadItem payload) {
            if (payload is DragAndDropPayloadItemFromTraderShopStorage) {
                return false;
            }

            return this.storageApi.CanDropItemToStorage(payload);
        }

        public virtual void OnDropItemToStorage(DragAndDropPayloadItem payload) {
            if (payload is DragAndDropPayloadItemFromTraderShopStorage) {
                return;
            }

            this.storageApi.DropItemToStorage(payload);
            
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);
        }

        private Widget BuildHeader(BuildContext context) {
            return new Row {
                CrossAxisSize      = AxisSize.Max,
                MainAxisSize       = AxisSize.Max,
                CrossAxisAlignment = CrossAxisAlignment.Center,
                MainAxisAlignment  = MainAxisAlignment.End,
                Size               = WidgetSize.Stretched,
                Children = {
                    new HeaderCurrencyWidget(SharedConstants.Game.Currencies.BADGES),
                    new HeaderCurrencyWidget(SharedConstants.Game.Currencies.BUCKS),
                    new HeaderCurrencyWidget(SharedConstants.Game.Currencies.CRYPT),
                },
            };
        }

        protected virtual Widget BuildItems(BuildContext context) {
            return new ScrollGridFlow {
                MaxCrossAxisExtent = (ITEM_WIDTH * 4) + 10,
                CrossAxisAlignment = CrossAxisAlignment.Start,
                Padding            = new RectPadding(0, 0, 30, 500),

                ChildrenBuilder = () => this.EnumerateItems()
                    .Select(it => (description: it, asset: this.GetItemAsset(it)))
                    .OrderByDescending(it => it.asset.Grouping)
                    .ThenByDescending(it => it.asset.rarity)
                    .ThenBy(it => it.asset.ItemKey)
                    .GroupBy(it => it.asset.Grouping)
                    .SelectMany(it => Enumerable.Empty<Widget>()
                        .Prepend(this.BuildSeparator(it.Key))
                        .Concat(it.Select(a => this.BuildItem(a.description))))
                    .ToList(),
            };
        }

        private Widget BuildSeparator(ItemAssetGrouping grouping) {
            return new ItemGroupingSeparatorWidget {
                Grouping = grouping,
                Width    = ITEM_WIDTH * 4,
            };
        }

        public virtual bool ShowTakeAllButton   => false;
        public virtual bool ShowEquipBestButton => false;

        public virtual void TakeAll() {
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.TAKE_ALL);
        }

        public virtual void EquipBest() {
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.EQUIP_BEST);
        }

        protected abstract Widget BuildItem(ItemDescription itemDescription);

        protected abstract IEnumerable<ItemDescription> EnumerateItems();

        protected abstract ItemAsset GetItemAsset(ItemDescription itemDescription);

        protected struct ItemDescription {
            [CanBeNull] public GameNearbyItemModel     NearbyModel;
            [CanBeNull] public GameSnapshotLoadoutItem Item;
        }
    }
}