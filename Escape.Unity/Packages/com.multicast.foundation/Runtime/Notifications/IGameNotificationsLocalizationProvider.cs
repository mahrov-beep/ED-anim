namespace Multicast.Notifications {
    internal interface IGameNotificationsLocalizationProvider {
        public string LocalizeChannelName(string key);
        public string LocalizeChannelDescription(string key);
        public string LocalizeNotificationTitle(string key);
        public string LocalizeNotificationBody(string key);
    }
}