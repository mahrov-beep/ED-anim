namespace Multicast.Modules.Notifications.Dummy {
    using System;
    using Multicast.Notifications;

    internal class DummyNotificationsPlatform : IGameNotificationsPlatform {
        public event Action<IGameNotification> NotificationReceived = delegate { };

        public IGameNotification CreateNotification() {
            return new DummyNotification();
        }

        public void ScheduleNotification(IGameNotification gameNotification) {
        }

        public void CancelNotification(int notificationId) {
        }

        public void DismissNotification(int notificationId) {
        }

        public void CancelAllScheduledNotifications() {
        }

        public void DismissAllDisplayedNotifications() {
        }

        public IGameNotification GetLastNotification() {
            return null;
        }

        public void OnForeground() {
        }

        public void OnBackground() {
        }
    }
}