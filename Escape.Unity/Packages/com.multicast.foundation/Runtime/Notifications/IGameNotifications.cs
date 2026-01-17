namespace Multicast.Notifications {
    using System;
    using System.Collections.Generic;

    public interface IGameNotifications {
        void ScheduleNotification(string key, DateTime deliveryTime);
        void CancelNotification(string key);

        IEnumerable<string> EnumerateAllNotifications(bool includeDisabled = false);
    }
}