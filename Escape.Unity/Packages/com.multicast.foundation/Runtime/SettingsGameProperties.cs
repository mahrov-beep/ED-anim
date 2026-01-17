namespace Multicast {
    using GameProperties;

    public static class SettingsGameProperties {
        public static readonly BoolGamePropertyName Vibration         = new("Vibration", true);
        public static readonly BoolGamePropertyName HighQuality       = new("HighQuality", true);
        public static readonly BoolGamePropertyName BatterySave       = new("BatterySave");
        public static readonly BoolGamePropertyName FrameRateCounter  = new("FrameRateCounter");
        public static readonly BoolGamePropertyName PushNotifications = new("PushNotifications", true);
    }
}