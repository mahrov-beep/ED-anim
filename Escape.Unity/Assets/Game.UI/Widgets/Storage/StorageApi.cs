namespace Game.UI.Widgets.Storage {
    using GameInventory;
    using Multicast;
    using Quantum.Commands;
    using Services.Photon;
    using Views;

    public class StorageApi {
        [Inject] private PhotonService    photonService;
        [Inject] private GameInventoryApi gameInventoryApi;

        public bool CanDropItemToStorage(DragAndDropPayloadItem payload) {
            return this.gameInventoryApi.CanThrowItem(payload);
        }

        public void DropItemToStorage(DragAndDropPayloadItem payload) {
            this.gameInventoryApi.ThrowItem(payload);
        }

        public void DropAllItemsToStorage() {
            this.photonService.Runner?.Game.SendCommand(new ThrowAwayAllItemsFromTrashLoadoutCommand {
                IsStorage = true,
            });
        }

        public void EquipBest() {
            this.photonService.Runner?.Game.SendCommand(new PickUpBestFromNearbyItemBoxLoadoutCommand {
                EquipTrash = false,
            });
        }
    }
}