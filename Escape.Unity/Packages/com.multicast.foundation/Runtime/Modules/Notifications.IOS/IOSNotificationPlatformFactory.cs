#if UNITY_MOBILE_NOTIFICATIONS && UNITY_IOS
namespace Multicast.Modules.Notifications.IOS {
    using Multicast.Notifications;

    internal class IOSNotificationPlatformFactory : IGameNotificationsPlatformFactory {
        public IGameNotificationsPlatform Create(params GameNotificationChannel[] channels) {
            return new iOSNotificationsPlatform();
        }
    }
}
#endif