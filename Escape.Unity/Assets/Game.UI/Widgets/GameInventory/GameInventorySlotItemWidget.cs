namespace Game.UI.Widgets.GameInventory {
    using System.Collections.Generic;
    using System.Linq;
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine.Pool;
    using Views;
    using Views.GameInventory;

    [RequireFieldsInit]
    public abstract class GameInventorySlotItemWidget : StatefulWidget {
    }

    public abstract class GameInventorySlotItemState<TState> : ViewState<TState>, IGameInventorySlotItemState
        where TState : GameInventorySlotItemWidget {
        [Inject] protected GameInventoryApi gameInventoryApi;

        private readonly StateHolder attachmentsState;

        protected GameInventorySlotItemState() {
            this.attachmentsState = this.CreateChild(this.BuildAttachments);
        }

        public override WidgetViewReference View => this.AssignedSlot.GetVisual() switch {
            LoadoutSlotVisual.Mini => UiConstants.Views.GameInventory.SlotItemMini,
            LoadoutSlotVisual.PrimaryWeapon => UiConstants.Views.GameInventory.SlotItemPrimary,
            LoadoutSlotVisual.SecondaryWeapon => UiConstants.Views.GameInventory.SlotItemSecondary,
            LoadoutSlotVisual.MeleeWeapon => UiConstants.Views.GameInventory.SlotItemMelee,
            _ => UiConstants.Views.GameInventory.SlotItem,
        };

        protected abstract CharacterLoadoutSlots AssignedSlot { get; }
        protected abstract ItemAsset             ItemAsset    { get; }

        public string SlotType => EnumNames<CharacterLoadoutSlots>.GetName(this.AssignedSlot);
        public string ItemKey  => this.ItemAsset.ItemKey;

        public string ItemIcon => this.AssignedSlot.GetVisual() switch {
            LoadoutSlotVisual.MeleeWeapon or LoadoutSlotVisual.PrimaryWeapon or LoadoutSlotVisual.SecondaryWeapon => this.ItemAsset.IconLarge,
            _ => this.ItemAsset.Icon,
        };


        public string ItemRarity => EnumNames<ERarityType>.GetName(this.ItemAsset.rarity);
        
        public IState Attachments => this.attachmentsState.Value;

        public virtual bool IsSelected      => false;
        public virtual bool IsBlocked       => false;

        public abstract int UsagesRemaining { get; }
        public abstract int UsagesMax       { get; }

        public abstract void Select();

        public abstract DragAndDropPayloadItem GetDragAndDropItemPayload();

        public abstract bool IsWeaponAttachment(DragAndDropPayloadItem payload);
        
        public abstract bool CanAssignItem(DragAndDropPayloadItem payload);
        public abstract bool CanDropItem(DragAndDropPayloadItem payload);

        public abstract void OnAssignItem(DragAndDropPayloadItem payload);

        protected abstract Widget BuildAttachments(BuildContext context);

        protected Widget BuildAttachmentsLayout(IEnumerable<Widget> attachments, int countOnFirstLine) {
            using (ListPool<Widget>.Get(out var list)) {
                list.AddRange(attachments);

                return new Column {
                    MainAxisAlignment  = MainAxisAlignment.End,
                    CrossAxisAlignment = CrossAxisAlignment.End,
                    CrossAxisSize      = AxisSize.Max,
                    MainAxisSize       = AxisSize.Max,
                    Children = {
                        new Row {
                            MainAxisAlignment  = MainAxisAlignment.End,
                            CrossAxisAlignment = CrossAxisAlignment.End,
                            Children = {
                                list.Skip(countOnFirstLine),
                            },
                        },
                        new Row {
                            MainAxisAlignment  = MainAxisAlignment.End,
                            CrossAxisAlignment = CrossAxisAlignment.End,
                            Children = {
                                list.Take(countOnFirstLine),
                            },
                        },
                    },
                };
            }
        }
    }
}