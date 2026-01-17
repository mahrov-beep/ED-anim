#if UNITY_MOBILE_NOTIFICATIONS && UNITY_IOS
namespace Multicast.Modules.Notifications.IOS {
    using System;
    using Multicast.Notifications;
    using global::Unity.Notifications.iOS;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// iOS implementation of <see cref="IGameNotificationsPlatform"/>.
    /// </summary>
    internal class iOSNotificationsPlatform : IGameNotificationsPlatform<iOSGameNotification>,
        IDisposable {
        /// <inheritdoc />
        public event Action<IGameNotification> NotificationReceived;

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSNotificationsPlatform"/>.
        /// </summary>
        public iOSNotificationsPlatform() {
            iOSNotificationCenter.OnNotificationReceived += this.OnLocalNotificationReceived;
        }

        /// <inheritdoc />
        public void ScheduleNotification(IGameNotification gameNotification) {
            if (gameNotification == null) {
                throw new ArgumentNullException(nameof(gameNotification));
            }

            if (!(gameNotification is iOSGameNotification notification)) {
                throw new InvalidOperationException(
                    "Notification provided to ScheduleNotification isn't an iOSGameNotification.");
            }

            this.ScheduleNotification(notification);
        }

        /// <inheritdoc />
        public void ScheduleNotification(iOSGameNotification notification) {
            if (notification == null) {
                throw new ArgumentNullException(nameof(notification));
            }

            iOSNotificationCenter.ScheduleNotification(notification.InternalNotification);
            notification.OnScheduled();
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="T:NotificationSamples.Android.AndroidNotification" />.
        /// </summary>
        IGameNotification IGameNotificationsPlatform.CreateNotification() {
            return this.CreateNotification();
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="T:NotificationSamples.Android.AndroidNotification" />.
        /// </summary>
        public iOSGameNotification CreateNotification() {
            return new iOSGameNotification();
        }

        /// <inheritdoc />
        public void CancelNotification(int notificationId) {
            iOSNotificationCenter.RemoveScheduledNotification(notificationId.ToString());
        }

        /// <inheritdoc />
        public void DismissNotification(int notificationId) {
            iOSNotificationCenter.RemoveDeliveredNotification(notificationId.ToString());
        }

        /// <inheritdoc />
        public void CancelAllScheduledNotifications() {
            iOSNotificationCenter.RemoveAllScheduledNotifications();
        }

        /// <inheritdoc />
        public void DismissAllDisplayedNotifications() {
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
        }

        /// <inheritdoc />
        IGameNotification IGameNotificationsPlatform.GetLastNotification() {
            return this.GetLastNotification();
        }

        /// <inheritdoc />
        public iOSGameNotification GetLastNotification() {
            var notification = iOSNotificationCenter.GetLastRespondedNotification();

            if (notification != null) {
                return new iOSGameNotification(notification);
            }

            return null;
        }

        /// <summary>
        /// Clears badge count.
        /// </summary>
        public void OnForeground() {
            iOSNotificationCenter.ApplicationBadge = 0;
        }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public void OnBackground() {
        }

        /// <summary>
        /// Unregister delegates.
        /// </summary>
        public void Dispose() {
            iOSNotificationCenter.OnNotificationReceived -= this.OnLocalNotificationReceived;
        }

        // Event handler for receiving local notifications.
        private void OnLocalNotificationReceived(iOSNotification notification) {
            // Create a new AndroidGameNotification out of the delivered notification, but only
            // if the event is registered
            this.NotificationReceived?.Invoke(new iOSGameNotification(notification));
        }
    }
}
#endif