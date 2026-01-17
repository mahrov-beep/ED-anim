namespace Multicast.Modules.Notifications {
    using CodeWriter.ViewBinding;
    using Multicast.Notifications;

    internal class GameNotificationsLocalizationProvider : IGameNotificationsLocalizationProvider {
        public string LocalizeChannelName(string key) {
            return BindingsLocalization.Localize($"NOTIFICATION_CHANNEL_NAME_{key}");
        }

        public string LocalizeChannelDescription(string key) {
            return BindingsLocalization.Localize($"NOTIFICATION_CHANNEL_DESC_{key}");
        }

        public string LocalizeNotificationTitle(string key) {
            return BindingsLocalization.Localize($"NOTIFICATION_TITLE_{key}");
        }

        public string LocalizeNotificationBody(string key) {
            return BindingsLocalization.Localize($"NOTIFICATION_BODY_{key}");
        }
    }
}