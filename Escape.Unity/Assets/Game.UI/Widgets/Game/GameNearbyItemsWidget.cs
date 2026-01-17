namespace Game.UI.Widgets.Game {
    using System;
    using Domain.GameInventory;
    using Domain.ItemBoxStorage;
    using GameInventory;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Game;

    [RequireFieldsInit]
    public class GameNearbyItemsWidget : StatefulWidget {
        public GameNearbyItemBoxModel ItemBoxModel;
    }

    public class GameNearbyItemsState : ViewState<GameNearbyItemsWidget>, IGameNearbyItemsState {
        [Inject] private PhotonService       photonService;
        [Inject] private GameNearbyItemsApi  nearbyItemsApi;
        [Inject] private ItemBoxStorageModel itemBoxStorageModel;

        private readonly StateHolder itemsState;

        private GameNearbyItemBoxModel Model => this.Widget.ItemBoxModel;

        public bool CanEquipBest => this.Model.CanEquipBest;
        public bool CanOpenClose => this.Model.CanOpen;
        
        public IState ItemsState   => this.itemsState.Value;

        public bool IsBackpack => this.Model.IsBackpack;

        public GameNearbyItemsState() {
            this.itemsState = this.CreateChild(this.BuildItems);
        }

        public override WidgetSize CalculateSize() {
            var size = base.CalculateSize();

            return WidgetSize.StackY(size, this.itemsState.Value.Size);
        }

        private Widget BuildItems(BuildContext context) {
            if (!this.Model.IsOpenedByMe) {
                return new Empty();
            }

            return new GameInventoryTetrisWidget {
                UpdatedFrame          = this.itemBoxStorageModel.UpdatedFrame,
                NoDraggingInInventory = false,
                Width                 = this.itemBoxStorageModel.Width,
                Height                = this.itemBoxStorageModel.Height,
                Items                 = this.itemBoxStorageModel.EnumerateItems(),
                Source                = TetrisSource.Storage,
                InShop                = false,
                MaxHeight             = 7,
            };
        }

        public override WidgetViewReference View => this.Model.IsOpenedByMe
            ? UiConstants.Views.Game.NearbyItemsOpened
            : UiConstants.Views.Game.NearbyItemsClosed;

        public void EquipBest() {
            this.photonService.Runner?.Game.SendCommand(new PickUpBestFromNearbyItemBoxLoadoutCommand {
                IsBackpack              = this.Model.IsBackpack,
                NeedToRemoveFromStorage = false,
            });
        }

        public void OpenClose() {
            if (this.Model.IsOpenedByMe) {
                this.photonService.Runner?.Game.SendCommand(new CloseItemBoxCommand());
            }
            else if (!this.Model.IsOpenedByOtherPlayer) {
                this.photonService.Runner?.Game.SendCommand(new OpenItemBoxCommand {
                    OpenBackpack = this.Model.IsBackpack,
                });
            }
        }
    }
}