namespace Game.UI.Widgets.Storage {
    using Multicast;
    using Multicast.Numerics;
    using Quantum;
    using UniMob.UI;
    using Views;
    using Views.Storage;

    public interface IStorageItemWidget : Widget {
    }

    public abstract class StorageItemState<TWidget> : ViewState<TWidget>, IStorageItemState where TWidget : IStorageItemWidget {
        private readonly StateHolder detailsState;

        protected StorageItemState() {
            this.detailsState = this.CreateChild(this.BuildDetails);
        }

        public override WidgetViewReference View => this.ItemAsset switch {
            WeaponItemAsset => UiConstants.Views.Storage.ItemX2,
            _ => UiConstants.Views.Storage.Item,
        };

        public string ItemKey => this.ItemAsset.ItemKey;

        public string ItemIcon => this.ItemAsset switch {
            WeaponItemAsset => this.ItemAsset.IconLarge,
            _ => this.ItemAsset.Icon,
        };

        public string ItemRarity => EnumNames<ERarityType>.GetName(this.ItemAsset.rarity);

        public virtual Cost ItemCost => Cost.Empty;

        public virtual bool Draggable => true;

        public IState Details => this.detailsState.Value;

        public virtual float Weight => 0f;

        protected abstract ItemAsset ItemAsset { get; }

        public abstract DragAndDropPayloadItem GetDragAndDropItemPayload();

        public abstract void Select();

        public abstract void OnDoubleClick();

        protected abstract Widget BuildDetails(BuildContext context);
    }
}