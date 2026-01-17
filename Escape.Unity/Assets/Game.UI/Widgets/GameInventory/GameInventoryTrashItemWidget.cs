namespace Game.UI.Widgets.GameInventory {
    using Controllers.Features.SelectedItemInfo;
    using Controllers.Features.GameInventory;
    using Domain;
    using Domain.GameInventory;
    using Items;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using SoundEffects;
    using UniMob.UI;
    using Views;
    using Views.GameInventory;

    [RequireFieldsInit]
    public class GameInventoryTrashItemWidget : StatefulWidget {
        public int                         IndexI;
        public int                         IndexJ;
        public GameInventoryTrashItemModel Model;
        public bool                        IsHudButton;
        public bool                        NoDragging;
        public TetrisSource                Source;
        public bool                        InShop;
    }

    public class GameInventoryTrashItemState : ViewState<GameInventoryTrashItemWidget>, IGameInventoryTrashItemState {
        [Inject] private PhotonService      photonService;
        [Inject] private GameInventoryModel gameInventoryModel;
        [Inject] private GameInventoryApi   gameInventoryApi;

        private readonly StateHolder detailsState;

        public GameInventoryTrashItemState() {
            this.detailsState = this.CreateChild(this.BuildDetails);
        }

        public override WidgetViewReference View {
            get {
                if (this.Widget.IsHudButton) {
                    return UiConstants.Views.GameInventory.TrashButtonItem;
                }

                if (this.ItemAsset.Width <= 2 && this.ItemAsset.Height <= 1) {
                    return UiConstants.Views.GameInventory.TrashItemMini;
                }

                return UiConstants.Views.GameInventory.TrashItem;
            }
        }

        private EntityRef ItemEntity => this.Widget.Model.ItemEntity;
        private ItemAsset ItemAsset  => this.Widget.Model.ItemAsset;
        private bool     IsUsageDisabled => this.gameInventoryModel.IsUsageLocked;

        public string ItemKey          => this.ItemAsset.ItemKey;
        public string ItemIcon         => this.ItemAsset.IconLarge;
        public bool   IsSelected       => this.gameInventoryModel.SelectedItem == this.ItemEntity;
        public bool   Rotated          => this.Widget.Model.Rotated.Value;
        public bool   IsHudButton      => this.Widget.IsHudButton;
        public bool   UseOnHoldStart   => !this.Widget.IsHudButton && !this.IsUsageDisabled && this.ItemAsset is HealBoxItemAsset;
        public float  HoldToUseSeconds => this.ItemAsset is UsableItemAsset usableItemAsset ? usableItemAsset.useDurationSeconds.AsFloat : 0f;

        public int UsagesRemaining => this.Widget.Model.RemainingUsages.Value;
        public int UsagesMax       => this.ItemAsset.MaxUsages;

        public bool   CanBeUsed  => !this.IsUsageDisabled && this.Widget.Model.CanBeUsed.Value;
        public float  Weight     => this.Widget.Model.Weight.Value;
        public string ItemRarity => EnumNames<ERarityType>.GetName(this.ItemAsset.rarity);

        public IState Details => this.detailsState.Value;

        public override WidgetSize CalculateSize() {
            var size = base.CalculateSize();

            var width  = this.ItemAsset.Width * CoreConstants.Tetris.CELL_SIZE;
            var height = this.ItemAsset.Height * CoreConstants.Tetris.CELL_SIZE;
            
            size = new WidgetSize(width, height, width, height);

            return size;
        }
        
        public void Select() {
            SelectedItemInfoFeatureEvents.Select.Raise(new SelectedItemInfoFeatureEvents.SelectArgs {
                ItemKey             = this.ItemAsset.ItemKey,
                ItemEntity          = this.ItemEntity,
                ItemSnapshot        = null,
                Position            = WidgetPosition.Position.Left,
                IsTakeButtonVisible = false,
            });
        }
        
        public void StartHold() {
            if (this.IsUsageDisabled || !this.CanBeUsed) {
                return;
            }

            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.USE_KIT);
        }

        public void Use() {
            if (this.IsUsageDisabled || !this.CanBeUsed) {
                return;
            }

            this.photonService.Runner?.Game.SendCommand(new UseItemCommand {
                ItemEntity = this.ItemEntity,
            });

            if (!this.Widget.IsHudButton && this.Widget.Model.ItemAsset is HealBoxItemAsset) {
                GameInventoryFeatureEvents.Close.Raise();
            }
        }

        public DragAndDropPayloadItem GetDragAndDropItemPayload() {
            if (this.Widget.NoDragging) return null;

            if (this.photonService.PredictedFrame is not { } f) {
                return default;
            }

            var item = f.Get<Item>(this.ItemEntity);

            return new DragAndDropPayloadItemEntityFromTetris {
                ItemEntity = this.ItemEntity,
                ItemGuid   = item.MetaGuid,
                Source     = this.Widget.Source,
            };
        }

        public void DoubleClick() {
            this.gameInventoryApi.FastItemMoveToTetris(this.ItemEntity, this.Widget.Source, this.Widget.InShop);
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.DRAG_ITEM);
        }

        private Widget BuildDetails(BuildContext context) {
            return new EntityItemDetailsWidget {
                ItemEntity = this.ItemEntity,
            };
        }
    }
}