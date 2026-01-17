namespace Multicast.Notifications {
    internal interface IGameNotificationsPlatformFactory {
        IGameNotificationsPlatform Create(params GameNotificationChannel[] channels);
    }
}