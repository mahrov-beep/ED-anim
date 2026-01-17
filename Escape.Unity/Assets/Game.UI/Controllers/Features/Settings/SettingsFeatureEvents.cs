namespace Game.UI.Controllers.Features.Settings {
    using Multicast;

    public static class SettingsFeatureEvents {
        public static readonly EventSource Open  = new();
        public static readonly EventSource Close = new();
    }
}
