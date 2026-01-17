namespace Game.UI.Widgets.GameInventory {
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using Views;
    using Views.GameInventory;

    [RequireFieldsInit]
    public abstract class GameInventoryWeaponAttachmentSlotItemWidget : StatefulWidget {
    }

    public abstract class GameInventoryWeaponAttachmentSlotItemState<TState> : ViewState<TState>, IGameInventoryWeaponAttachmentSlotItemState
        where TState : GameInventoryWeaponAttachmentSlotItemWidget {
        [Inject] protected GameInventoryApi gameInventoryApi;

        public override WidgetViewReference View => UiConstants.Views.GameInventory.WeaponAttachmentSlot;

        protected abstract ItemAsset             ItemAsset    { get; }
        protected abstract WeaponAttachmentSlots AssignedSlot { get; }

        public string WeaponSlotType => EnumNames<WeaponAttachmentSlots>.GetName(this.AssignedSlot);
        public string ItemKey        => this.ItemAsset.ItemKey;
        public string ItemIcon       => this.ItemAsset.Icon;
        public string ItemRarity     => EnumNames<ERarityType>.GetName(this.ItemAsset.rarity);

        public virtual bool IsSelected => false;

        public abstract int UsagesRemaining { get; }
        public abstract int UsagesMax       { get; }

        public abstract DragAndDropPayloadItem GetDragAndDropItemPayload();

        public abstract bool CanAssignItem(DragAndDropPayloadItem payload);
        public abstract void OnAssignItem(DragAndDropPayloadItem payload);

        public abstract void Select();
    }
}