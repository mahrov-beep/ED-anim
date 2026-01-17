namespace Multicast.Modules.Notifications {
    using System;
    using System.Collections.Generic;
    using Collections;
    using Multicast.Notifications;
    using UnityEngine;

    internal class GameNotifications : IGameNotifications {
        private readonly GameNotificationsManager               manager;
        private readonly LookupCollection<GameNotificationDef>  notificationsDef;
        private readonly IGameNotificationsLocalizationProvider localizationProvider;

        public GameNotifications(
            GameNotificationsManager manager,
            LookupCollection<GameNotificationDef> notificationsDef,
            IGameNotificationsLocalizationProvider localizationProvider) {
            this.manager              = manager;
            this.notificationsDef     = notificationsDef;
            this.localizationProvider = localizationProvider;
        }

        public void ScheduleNotification(string key, DateTime deliveryTime) {
            if (!this.notificationsDef.TryGet(key, out var def)) {
                Debug.LogError($"Failed to schedule {key} notification: def not found");
                return;
            }

            if (!def.enabled) {
                return;
            }

            var notification = this.manager.CreateNotification();

            notification.Id    = def.id;
            notification.Group = def.channel;
            notification.Title = this.localizationProvider.LocalizeNotificationTitle(def.key);
            notification.Body  = this.localizationProvider.LocalizeNotificationBody(def.key);

            notification.DeliveryTime     = deliveryTime;
            notification.ShouldAutoCancel = true;

            if (!string.IsNullOrEmpty(def.smallIcon)) {
                notification.SmallIcon = def.smallIcon;
            }

            if (!string.IsNullOrEmpty(def.largeIcon)) {
                notification.LargeIcon = def.largeIcon;
            }

            this.manager.ScheduleNotification(notification);
        }

        public void CancelNotification(string key) {
            if (!this.notificationsDef.TryGet(key, out var def)) {
                Debug.LogError($"Failed to cancel {key} notification: def not found");
                return;
            }

            this.manager.CancelNotification(def.id);
        }

        public IEnumerable<string> EnumerateAllNotifications(bool includeDisabled = false) {
            foreach (var def in this.notificationsDef.Items) {
                if (includeDisabled || def.enabled) {
                    yield return def.key;
                }
            }
        }
    }
}