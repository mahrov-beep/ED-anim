namespace Multicast.Notifications {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// Global notifications manager that serves as a wrapper for multiple platforms' notification systems.
    /// </summary>
    internal class GameNotificationsManager : MonoBehaviour {
        public static bool NotificationsEnabled = true;
        
        // Default filename for notifications serializer
        private const string DefaultFilename = "notifications2.bin";

        // Minimum amount of time that a notification should be into the future before it's queued when we background.
        private static readonly TimeSpan MinimumNotificationTime = new TimeSpan(0, 0, 2);

        [SerializeField, Tooltip(
             "Check to make the notifications manager automatically set badge numbers so that they increment.\n" +
             "Schedule notifications with no numbers manually set to make use of this feature.")]
        private bool autoBadging = true;

        /// <summary>
        /// Event fired when a scheduled local notification is delivered while the app is in the foreground.
        /// </summary>
        public event Action<PendingNotification> LocalNotificationDelivered;

        /// <summary>
        /// Event fired when a queued local notification is cancelled because the application is in the foreground
        /// when it was meant to be displayed.
        /// </summary>
        /// <seealso cref="OperatingMode.Queue"/>
        public event Action<PendingNotification> LocalNotificationExpired;

        /// <summary>
        /// Gets the implementation of the notifications for the current platform;
        /// </summary>
        public IGameNotificationsPlatform Platform { get; private set; }

        /// <summary>
        /// Gets a collection of notifications that are scheduled or queued.
        /// </summary>
        public List<PendingNotification> PendingNotifications { get; private set; }

        /// <summary>
        /// Gets or sets the serializer to use to save pending notifications to disk if we're in
        /// <see cref="OperatingMode.RescheduleAfterClearing"/> mode.
        /// </summary>
        public IPendingNotificationsSerializer Serializer { get; set; }

        /// <summary>
        /// Gets whether this manager automatically increments badge numbers.
        /// </summary>
        public bool AutoBadging => this.autoBadging;

        /// <summary>
        /// Gets whether this manager has been initialized.
        /// </summary>
        public bool Initialized { get; private set; }

        // Flag set when we're in the foreground
        private bool inForeground = true;

        protected virtual void Awake() {
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// Clean up platform object if necessary
        /// </summary>
        protected virtual void OnDestroy() {
            if (this.Platform == null) {
                return;
            }

            this.Platform.NotificationReceived -= this.OnNotificationReceived;
            if (this.Platform is IDisposable disposable) {
                disposable.Dispose();
            }

            this.inForeground = false;
        }

        /// <summary>
        /// Check pending list for expired notifications, when in queue mode.
        /// </summary>
        protected virtual void Update() {
            if (this.PendingNotifications == null || this.PendingNotifications.Count == 0) {
                return;
            }

            // Check each pending notification for expiry, then remove it
            for (int i = this.PendingNotifications.Count - 1; i >= 0; --i) {
                PendingNotification queuedNotification = this.PendingNotifications[i];
                DateTime?           time               = queuedNotification.Notification.DeliveryTime;
                if (time != null && time < DateTime.Now) {
                    this.PendingNotifications.RemoveAt(i);
                    this.LocalNotificationExpired?.Invoke(queuedNotification);
                }
            }
        }

        /// <summary>
        /// Respond to application foreground/background events.
        /// </summary>
        protected void OnApplicationPause(bool paused) {
            if (this.Platform == null || !this.Initialized) {
                return;
            }

            this.inForeground = !paused;

            if (!paused) {
                SynchronizationContext.Current.Post(_ => this.OnForegrounding(), null);

                return;
            }

            this.Platform.OnBackground();

            // Filter out past events
            for (var i = this.PendingNotifications.Count - 1; i >= 0; i--) {
                PendingNotification pendingNotification = this.PendingNotifications[i];
                // Ignore already scheduled ones
                if (pendingNotification.Notification.Scheduled) {
                    continue;
                }

                // If a non-scheduled notification is in the past (or not within our threshold)
                // just remove it immediately
                if (pendingNotification.Notification.DeliveryTime != null &&
                    pendingNotification.Notification.DeliveryTime - DateTime.Now < MinimumNotificationTime) {
                    this.PendingNotifications.RemoveAt(i);
                }
            }

            // Sort notifications by delivery time, if no notifications have a badge number set
            bool noBadgeNumbersSet =
                this.PendingNotifications.All(notification => notification.Notification.BadgeNumber == null);

            if (noBadgeNumbersSet && this.AutoBadging) {
                this.PendingNotifications.Sort((a, b) => {
                    if (!a.Notification.DeliveryTime.HasValue) {
                        return 1;
                    }

                    if (!b.Notification.DeliveryTime.HasValue) {
                        return -1;
                    }

                    return a.Notification.DeliveryTime.Value.CompareTo(b.Notification.DeliveryTime.Value);
                });

                // Set badge numbers incrementally
                var badgeNum = 1;
                foreach (PendingNotification pendingNotification in this.PendingNotifications) {
                    if (pendingNotification.Notification.DeliveryTime.HasValue &&
                        !pendingNotification.Notification.Scheduled) {
                        pendingNotification.Notification.BadgeNumber = badgeNum++;
                    }
                }
            }

            for (int i = this.PendingNotifications.Count - 1; i >= 0; i--) {
                PendingNotification pendingNotification = this.PendingNotifications[i];
                // Ignore already scheduled ones
                if (pendingNotification.Notification.Scheduled) {
                    continue;
                }

                // Schedule it now
                if (NotificationsEnabled) {
                    this.Platform.ScheduleNotification(pendingNotification.Notification);
                }
            }

            // Clear badge numbers again (for saving)
            if (noBadgeNumbersSet && this.AutoBadging) {
                foreach (PendingNotification pendingNotification in this.PendingNotifications) {
                    if (pendingNotification.Notification.DeliveryTime.HasValue) {
                        pendingNotification.Notification.BadgeNumber = null;
                    }
                }
            }

            // Calculate notifications to save
            var notificationsToSave = new List<PendingNotification>(this.PendingNotifications.Count);
            foreach (PendingNotification pendingNotification in this.PendingNotifications) {
                // In reschedule mode, add ones that have been scheduled, are marked for
                // rescheduling, and that have a time
                if (pendingNotification.Notification.Scheduled &&
                    pendingNotification.Notification.DeliveryTime.HasValue) {
                    notificationsToSave.Add(pendingNotification);
                }
            }

            // Save to disk
            this.Serializer.Serialize(notificationsToSave);
        }

        /// <summary>
        /// Initialize the notifications manager.
        /// </summary>
        /// <param name="channels">An optional collection of channels to register, for Android</param>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has already been called.</exception>
        public void Initialize(IGameNotificationsPlatformFactory platformFactory, params GameNotificationChannel[] channels) {
            if (this.Initialized) {
                throw new InvalidOperationException("NotificationsManager already initialized.");
            }

            this.Initialized = true;

            Platform = platformFactory.Create(channels);

            if (this.Platform == null) {
                return;
            }

            this.PendingNotifications          =  new List<PendingNotification>();
            this.Platform.NotificationReceived += this.OnNotificationReceived;

            // Check serializer
            if (this.Serializer == null) {
                this.Serializer = new DefaultSerializer(Path.Combine(Application.persistentDataPath, DefaultFilename));
            }

            this.OnForegrounding();
        }

        /// <summary>
        /// Creates a new notification object for the current platform.
        /// </summary>
        /// <returns>The new notification, ready to be scheduled, or null if there's no valid platform.</returns>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public IGameNotification CreateNotification() {
            if (!this.Initialized) {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            return this.Platform?.CreateNotification();
        }

        /// <summary>
        /// Schedules a notification to be delivered.
        /// </summary>
        /// <param name="notification">The notification to deliver.</param>
        public PendingNotification ScheduleNotification(IGameNotification notification) {
            if (!this.Initialized) {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            if (notification == null || this.Platform == null) {
                return null;
            }

            // Also immediately schedule non-time based deliveries (for iOS)
            if (notification.DeliveryTime == null) {
                if (NotificationsEnabled) {
                    this.Platform.ScheduleNotification(notification);
                }
            }
            else if (!notification.Id.HasValue) {
                // Generate an ID for items that don't have one (just so they can be identified later)
                int id = Math.Abs(DateTime.Now.ToString("yyMMddHHmmssffffff").GetHashCode());
                notification.Id = id;
            }

            // Register pending notification
            var result = new PendingNotification(notification);
            this.PendingNotifications.Add(result);

            return result;
        }

        /// <summary>
        /// Cancels a scheduled notification.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to cancel.</param>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public void CancelNotification(int notificationId) {
            if (!this.Initialized) {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            if (this.Platform == null) {
                return;
            }

            // Remove the cancelled notification from scheduled list
            int index = this.PendingNotifications.FindIndex(scheduledNotification =>
                scheduledNotification.Notification.Id == notificationId);

            if (index >= 0) {
                this.PendingNotifications.RemoveAt(index);
            }
        }

        /// <summary>
        /// Cancels all scheduled notifications.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public void CancelAllNotifications() {
            if (!this.Initialized) {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            if (this.Platform == null) {
                return;
            }

            this.PendingNotifications.Clear();
        }

        /// <summary>
        /// Dismisses a displayed notification.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to dismiss.</param>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public void DismissNotification(int notificationId) {
            if (!this.Initialized) {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            this.Platform?.DismissNotification(notificationId);
        }

        /// <summary>
        /// Dismisses all displayed notifications.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public void DismissAllNotifications() {
            if (!this.Initialized) {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            this.Platform?.DismissAllDisplayedNotifications();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IGameNotification GetLastNotification() {
            if (!this.Initialized) {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            return this.Platform?.GetLastNotification();
        }

        /// <summary>
        /// Event fired by <see cref="Platform"/> when a notification is received.
        /// </summary>
        private void OnNotificationReceived(IGameNotification deliveredNotification) {
            // Ignore for background messages (this happens on Android sometimes)
            if (!this.inForeground) {
                return;
            }

            // Find in pending list
            int deliveredIndex =
                this.PendingNotifications.FindIndex(scheduledNotification =>
                    scheduledNotification.Notification.Id == deliveredNotification.Id);
            if (deliveredIndex >= 0) {
                this.LocalNotificationDelivered?.Invoke(this.PendingNotifications[deliveredIndex]);

                this.PendingNotifications.RemoveAt(deliveredIndex);
            }
        }

        // Clear foreground notifications and reschedule stuff from a file
        private void OnForegrounding() {
            this.PendingNotifications.Clear();

            this.Platform.OnForeground();

            // Deserialize saved items
            IList<IGameNotification> loaded = this.Serializer?.Deserialize(this.Platform);

            // Clear on foregrounding
            this.Platform.CancelAllScheduledNotifications();

            // Only reschedule in reschedule mode, and if we loaded any items
            if (loaded == null) {
                return;
            }

            // Reschedule notifications from deserialization
            foreach (IGameNotification savedNotification in loaded) {
                if (savedNotification.DeliveryTime > DateTime.Now) {
                    this.ScheduleNotification(savedNotification);
                }
            }
        }
    }
}