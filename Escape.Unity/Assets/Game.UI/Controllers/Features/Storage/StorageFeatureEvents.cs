namespace Game.UI.Controllers.Features.Storage {
    using Multicast;

    public static class StorageFeatureEvents {
        public static readonly EventSource Open             = new();
        public static readonly EventSource Close            = new();
        public static readonly EventSource IncrementLoadout = new();
        public static readonly EventSource DecrementLoadout = new();
    }
}