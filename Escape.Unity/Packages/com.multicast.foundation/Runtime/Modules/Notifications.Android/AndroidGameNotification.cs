#if UNITY_MOBILE_NOTIFICATIONS && UNITY_ANDROID
namespace Multicast.Modules.Notifications.Android {
    using System;
    using global::Unity.Notifications.Android;
    using Multicast.Notifications;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// Android specific implementation of <see cref="IGameNotification"/>.
    /// </summary>
    internal class AndroidGameNotification : IGameNotification {
        private AndroidNotification internalNotification;

        /// <summary>
        /// Gets the internal notification object used by the mobile notifications system.
        /// </summary>
        public AndroidNotification InternalNotification => this.internalNotification;

        /// <inheritdoc />
        /// <summary>
        /// On Android, if the ID isn't explicitly set, it will be generated after it has been scheduled.
        /// </summary>
        public int? Id { get; set; }

        /// <inheritdoc />
        public string Title {
            get => this.InternalNotification.Title;
            set => this.internalNotification.Title = value;
        }

        /// <inheritdoc />
        public string Body {
            get => this.InternalNotification.Text;
            set => this.internalNotification.Text = value;
        }

        /// <summary>
        /// Does nothing on Android.
        /// </summary>
        public string Subtitle {
            get => null;
            set { }
        }

        /// <inheritdoc />
        public string Data {
            get => this.InternalNotification.IntentData;
            set => this.internalNotification.IntentData = value;
        }

        /// <inheritdoc />
        /// <remarks>
        /// On Android, this represents the notification's channel, and is required. Will be configured automatically by
        /// <see cref="AndroidNotificationsPlatform"/> if <see cref="AndroidNotificationsPlatform.DefaultChannelId"/> is set
        /// </remarks>
        /// <value>The value of <see cref="DeliveredChannel"/>.</value>
        public string Group {
            get => this.DeliveredChannel;
            set => this.DeliveredChannel = value;
        }

        /// <inheritdoc />
        public int? BadgeNumber {
            get => this.internalNotification.Number != -1 ? this.internalNotification.Number : (int?) null;
            set => this.internalNotification.Number = value ?? -1;
        }

        /// <inheritdoc />
        public bool ShouldAutoCancel {
            get => this.InternalNotification.ShouldAutoCancel;
            set => this.internalNotification.ShouldAutoCancel = value;
        }

        /// <inheritdoc />
        public DateTime? DeliveryTime {
            get => this.InternalNotification.FireTime;
            set => this.internalNotification.FireTime = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the channel for this notification.
        /// </summary>
        public string DeliveredChannel { get; set; }

        /// <inheritdoc />
        public bool Scheduled { get; private set; }

        /// <inheritdoc />
        public string SmallIcon {
            get => this.InternalNotification.SmallIcon;
            set => this.internalNotification.SmallIcon = value;
        }

        /// <inheritdoc />
        public string LargeIcon {
            get => this.InternalNotification.LargeIcon;
            set => this.internalNotification.LargeIcon = value;
        }

        public Color? Color {
            get => this.InternalNotification.Color;
            set => this.internalNotification.Color = value;
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="AndroidGameNotification"/>.
        /// </summary>
        public AndroidGameNotification() {
            this.internalNotification = new AndroidNotification();
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="AndroidGameNotification"/> from a delivered notification
        /// </summary>
        /// <param name="deliveredNotification">The notification that has been delivered.</param>
        /// <param name="deliveredId">The ID of the delivered notification.</param>
        /// <param name="deliveredChannel">The channel the notification was delivered to.</param>
        internal AndroidGameNotification(AndroidNotification deliveredNotification, int deliveredId,
            string deliveredChannel) {
            this.internalNotification = deliveredNotification;
            this.Id                   = deliveredId;
            this.DeliveredChannel     = deliveredChannel;
        }

        /// <summary>
        /// Set the scheduled flag.
        /// </summary>
        internal void OnScheduled() {
            Assert.IsFalse(this.Scheduled);
            this.Scheduled = true;
        }
    }
}
#endif