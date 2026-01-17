namespace Multicast.Notifications {
    using System;

    /// <summary>
    /// Represents a notification that was scheduled with <see cref="GameNotificationsManager.ScheduleNotification"/>.
    /// </summary>
    internal class PendingNotification {
        /// <summary>
        /// The scheduled notification.
        /// </summary>
        public readonly IGameNotification Notification;

        /// <summary>
        /// Instantiate a new instance of <see cref="PendingNotification"/> from a <see cref="IGameNotification"/>.
        /// </summary>
        /// <param name="notification">The notification to create from.</param>
        public PendingNotification(IGameNotification notification) {
            this.Notification = notification ?? throw new ArgumentNullException(nameof(notification));
        }
    }
}