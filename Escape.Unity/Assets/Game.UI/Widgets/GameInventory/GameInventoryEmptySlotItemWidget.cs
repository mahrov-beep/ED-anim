namespace Game.UI.Widgets.GameInventory {
    using Domain;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using SoundEffects;
    using UniMob.UI;
    using Views;
    using Views.GameInventory;

    [RequireFieldsInit]
    public class GameInventoryEmptySlotItemWidget : StatefulWidget {
        public CharacterLoadoutSlots SlotType;
    }

    public class GameInventoryEmptySlotItemState : ViewState<GameInventoryEmptySlotItemWidget>, IGameInventoryEmptySlotItemState {
        [Inject] private GameInventoryApi gameInventoryApi;
        [Inject] private PhotonService    photonService;

        public override WidgetViewReference View => this.Widget.SlotType.GetVisual() switch {
            LoadoutSlotVisual.Mini => UiConstants.Views.GameInventory.SlotEmptyItemMini,
            LoadoutSlotVisual.PrimaryWeapon => UiConstants.Views.GameInventory.SlotEmptyItemPrimary,
            LoadoutSlotVisual.SecondaryWeapon => UiConstants.Views.GameInventory.SlotEmptyItemSecondary,
            LoadoutSlotVisual.MeleeWeapon => UiConstants.Views.GameInventory.SlotEmptyItemMelee,
            _ => UiConstants.Views.GameInventory.SlotEmptyItem,
        };

        public string SlotType => EnumNames<CharacterLoadoutSlots>.GetName(this.Widget.SlotType);

        public bool CanDrop(DragAndDropPayloadItem payload) {
            if (payload is DragAndDropPayloadItemEntityFromTetris fromTetris) {
                if (!this.gameInventoryApi.HasItemAtSlot(this.Widget.SlotType, out var entityRef)) {
                    return true;
                }

                return this.gameInventoryApi.CanSwapItem(this.Widget.SlotType, fromTetris.ItemEntity);
            }

            return true;
        }
        
        public bool CanAssignItem(DragAndDropPayloadItem payload) {
            return this.gameInventoryApi.CanAssignItemToSlot(payload, this.Widget.SlotType);
        }

        public void OnAssignItem(DragAndDropPayloadItem payload) {
            if (!this.CanDrop(payload)) {
                return;
            }
            
            this.gameInventoryApi.AssignItemToSlot(payload, this.Widget.SlotType);
            
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.Equip);
        }
    }
}