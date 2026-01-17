namespace Multicast.Modules.Notifications.Dummy {
    using Multicast.Notifications;

    internal class DummyNotificationPlatformFactory : IGameNotificationsPlatformFactory {
        public IGameNotificationsPlatform Create(params GameNotificationChannel[] channels) {
            return new DummyNotificationsPlatform();
        }
    }
}