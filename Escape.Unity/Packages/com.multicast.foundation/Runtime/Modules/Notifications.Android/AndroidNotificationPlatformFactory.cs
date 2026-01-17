#if UNITY_MOBILE_NOTIFICATIONS && UNITY_ANDROID
namespace Multicast.Modules.Notifications.Android {
    using System.Linq;
    using global::Unity.Notifications.Android;
    using Multicast.Notifications;

    internal class AndroidNotificationPlatformFactory : IGameNotificationsPlatformFactory {
        public IGameNotificationsPlatform Create(params GameNotificationChannel[] channels) {
            var platform = new AndroidNotificationsPlatform();

            // Register the notification channels
            var doneDefault = false;
            foreach (GameNotificationChannel notificationChannel in channels) {
                if (!doneDefault) {
                    doneDefault               = true;
                    platform.DefaultChannelId = notificationChannel.Id;
                }

                long[] vibrationPattern = null;
                if (notificationChannel.VibrationPattern != null)
                    vibrationPattern = notificationChannel.VibrationPattern.Select(v => (long) v).ToArray();

                // Wrap channel in Android object
                var androidChannel = new AndroidNotificationChannel(notificationChannel.Id, notificationChannel.Name,
                    notificationChannel.Description,
                    (Importance) notificationChannel.Style) {
                    CanBypassDnd         = notificationChannel.HighPriority,
                    CanShowBadge         = notificationChannel.ShowsBadge,
                    EnableLights         = notificationChannel.ShowLights,
                    EnableVibration      = notificationChannel.Vibrates,
                    LockScreenVisibility = (LockScreenVisibility) notificationChannel.Privacy,
                    VibrationPattern     = vibrationPattern
                };

                AndroidNotificationCenter.RegisterNotificationChannel(androidChannel);
            }

            return platform;
        }
    }
}
#endif