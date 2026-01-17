#if UNITY_MOBILE_NOTIFICATIONS && UNITY_IOS
namespace Multicast.Modules.Notifications.IOS {
    using System;
    using global::Unity.Notifications.iOS;
    using Multicast.Notifications;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// iOS implementation of <see cref="IGameNotification"/>.
    /// </summary>
    internal class iOSGameNotification : IGameNotification {
        private readonly iOSNotification internalNotification;

        /// <summary>
        /// Gets the internal notification object used by the mobile notifications system.
        /// </summary>
        public iOSNotification InternalNotification => this.internalNotification;

        /// <inheritdoc />
        /// <remarks>
        /// Internally stored as a string. Gets parsed to an integer when retrieving.
        /// </remarks>
        /// <value>The identifier as an integer, or null if the identifier couldn't be parsed as a number.</value>
        public int? Id {
            get {
                if (!int.TryParse(this.internalNotification.Identifier, out int value)) {
                    Debug.LogWarning("Internal iOS notification's identifier isn't a number.");
                    return null;
                }

                return value;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.internalNotification.Identifier = value.Value.ToString();
            }
        }

        /// <inheritdoc />
        public string Title {
            get => this.internalNotification.Title;
            set => this.internalNotification.Title = value;
        }

        /// <inheritdoc />
        public string Body {
            get => this.internalNotification.Body;
            set => this.internalNotification.Body = value;
        }

        /// <inheritdoc />
        public string Subtitle {
            get => this.internalNotification.Subtitle;
            set => this.internalNotification.Subtitle = value;
        }

        /// <inheritdoc />
        public string Data {
            get => this.internalNotification.Data;
            set => this.internalNotification.Data = value;
        }

        /// <inheritdoc />
        /// <remarks>
        /// On iOS, this represents the notification's Category Identifier.
        /// </remarks>
        /// <value>The value of <see cref="CategoryIdentifier"/>.</value>
        public string Group {
            get => this.CategoryIdentifier;
            set => this.CategoryIdentifier = value;
        }

        /// <inheritdoc />
        public int? BadgeNumber {
            get => this.internalNotification.Badge != -1 ? this.internalNotification.Badge : (int?) null;
            set => this.internalNotification.Badge = value ?? -1;
        }

        /// <inheritdoc />
        public bool ShouldAutoCancel { get; set; }

        /// <inheritdoc />
        public bool Scheduled { get; private set; }

        /// <inheritdoc />
        /// <remarks>
        /// <para>On iOS, setting this causes the notification to be delivered on a calendar time.</para>
        /// <para>If it has previously been manually set to a different type of trigger, or has not been set before,
        /// this returns null.</para>
        /// <para>The millisecond component of the provided DateTime is ignored.</para>
        /// </remarks>
        /// <value>A <see cref="DateTime"/> representing the delivery time of this message, or null if
        /// not set or the trigger isn't a <see cref="iOSNotificationCalendarTrigger"/>.</value>
        public DateTime? DeliveryTime {
            get {
                if (!(this.internalNotification.Trigger is iOSNotificationCalendarTrigger calendarTrigger)) {
                    return null;
                }

                DateTime now = DateTime.Now;
                var result = new DateTime
                (
                    calendarTrigger.Year ?? now.Year,
                    calendarTrigger.Month ?? now.Month,
                    calendarTrigger.Day ?? now.Day,
                    calendarTrigger.Hour ?? now.Hour,
                    calendarTrigger.Minute ?? now.Minute,
                    calendarTrigger.Second ?? now.Second,
                    DateTimeKind.Local
                );

                return result;
            }
            set {
                if (!value.HasValue) {
                    return;
                }

                DateTime date = value.Value.ToLocalTime();

                this.internalNotification.Trigger = new iOSNotificationCalendarTrigger {
                    Year   = date.Year,
                    Month  = date.Month,
                    Day    = date.Day,
                    Hour   = date.Hour,
                    Minute = date.Minute,
                    Second = date.Second
                };
            }
        }

        /// <summary>
        /// The category identifier for this notification.
        /// </summary>
        public string CategoryIdentifier {
            get => this.internalNotification.CategoryIdentifier;
            set => this.internalNotification.CategoryIdentifier = value;
        }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public string SmallIcon {
            get => null;
            set { }
        }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public string LargeIcon {
            get => null;
            set { }
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSGameNotification"/>.
        /// </summary>
        public iOSGameNotification() {
            this.internalNotification = new iOSNotification(GenerateUniqueID()) {
                ShowInForeground = true // Deliver in foreground by default
            };

            string GenerateUniqueID() {
                return DateTime.Now.Ticks.GetHashCode().ToString();
            }
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSGameNotification"/> from a delivered notification.
        /// </summary>
        /// <param name="internalNotification">The delivered notification.</param>
        internal iOSGameNotification(iOSNotification internalNotification) {
            this.internalNotification = internalNotification;
        }

        /// <summary>
        /// Mark this notifications scheduled flag.
        /// </summary>
        internal void OnScheduled() {
            Assert.IsFalse(this.Scheduled);
            this.Scheduled = true;
        }
    }
}
#endif