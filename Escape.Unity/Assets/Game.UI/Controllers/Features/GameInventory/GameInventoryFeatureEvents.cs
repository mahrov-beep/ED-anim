namespace Game.UI.Controllers.Features.GameInventory {
    using Multicast;

    public static class GameInventoryFeatureEvents {
        public static readonly EventSource Open = new();
        public static readonly EventSource Close = new();
    }
}