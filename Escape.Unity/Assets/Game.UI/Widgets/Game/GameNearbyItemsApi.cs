namespace Game.UI.Widgets.Game {
    using GameInventory;
    using Multicast;
    using Views;

    public class GameNearbyItemsApi {
        [Inject] private GameInventoryApi gameInventoryApi;

        public bool CanDropItemToNearby(DragAndDropPayloadItem payload) {
            return this.gameInventoryApi.CanThrowItem(payload);
        }

        public void OnDropItemToNearby(DragAndDropPayloadItem payload) {
            this.gameInventoryApi.ThrowItem(payload);
        }
    }
}