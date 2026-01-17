namespace Multicast.Notifications {
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Standard serializer used by the <see cref="GameNotificationsManager"/> if no others
    /// are provided. Saves a simple binary format.
    /// </summary>
    internal class DefaultSerializer : IPendingNotificationsSerializer {
        private const byte Version = 1;

        private readonly string filename;

        /// <summary>
        /// Instantiate a new instance of <see cref="DefaultSerializer"/>.
        /// </summary>
        /// <param name="filename">The filename to save to. This should be an absolute path.</param>
        public DefaultSerializer(string filename) {
            this.filename = filename;
        }

        /// <inheritdoc />
        public void Serialize(IList<PendingNotification> notifications) {
            try {
                using (var file = new FileStream(this.filename, FileMode.Create)) {
                    using (var writer = new BinaryWriter(file)) {
                        // Write version number
                        writer.Write(Version);

                        // Write list length
                        writer.Write(notifications.Count);

                        // Write each item
                        foreach (PendingNotification notificationToSave in notifications) {
                            IGameNotification notification = notificationToSave.Notification;

                            // ID
                            writer.Write(notification.Id.HasValue);
                            if (notification.Id.HasValue) {
                                writer.Write(notification.Id.Value);
                            }

                            // Title
                            writer.Write(notification.Title ?? "");

                            // Body
                            writer.Write(notification.Body ?? "");

                            // Subtitle
                            writer.Write(notification.Subtitle ?? "");

                            // Group
                            writer.Write(notification.Group ?? "");

                            // Should Auto Cancel
                            writer.Write(notification.ShouldAutoCancel);

                            // Data
                            writer.Write(notification.Data ?? "");

                            // Badge
                            writer.Write(notification.BadgeNumber.HasValue);
                            if (notification.BadgeNumber.HasValue) {
                                writer.Write(notification.BadgeNumber.Value);
                            }

                            // Time (must have a value)
                            writer.Write(notification.DeliveryTime.Value.Ticks);

                            // Small Icon
                            writer.Write(notification.SmallIcon ?? "");

                            // Large Icon
                            writer.Write(notification.LargeIcon ?? "");
                        }
                    }
                }
            }
            catch (IOException) {
                // Fail
            }
        }

        /// <inheritdoc />
        public IList<IGameNotification> Deserialize(IGameNotificationsPlatform platform) {
            if (!File.Exists(this.filename)) {
                return null;
            }

            try {
                using (var file = new FileStream(this.filename, FileMode.Open)) {
                    if (file.Length == 0) {
                        return null;
                    }

                    using (var reader = new BinaryReader(file)) {
                        // Version
                        var version = reader.ReadByte();

                        // Length
                        int numElements = reader.ReadInt32();

                        var result = new List<IGameNotification>(numElements);
                        for (var i = 0; i < numElements; ++i) {
                            IGameNotification notification = platform.CreateNotification();
                            bool              hasValue;

                            // ID
                            hasValue = reader.ReadBoolean();
                            if (hasValue) {
                                notification.Id = reader.ReadInt32();
                            }

                            // Title
                            notification.Title = reader.ReadString();

                            // Body
                            notification.Body = reader.ReadString();

                            // Body
                            notification.Subtitle = reader.ReadString();

                            // Group
                            notification.Group = reader.ReadString();

                            // Should Auto Cancel
                            notification.ShouldAutoCancel = reader.ReadBoolean();

                            // Data, introduced in version 1
                            if (version > 0)
                                notification.Data = reader.ReadString();

                            // Badge
                            hasValue = reader.ReadBoolean();
                            if (hasValue) {
                                notification.BadgeNumber = reader.ReadInt32();
                            }

                            // Time
                            notification.DeliveryTime = new DateTime(reader.ReadInt64(), DateTimeKind.Local);

                            // Small icon
                            notification.SmallIcon = reader.ReadString();
                            if (string.IsNullOrEmpty(notification.SmallIcon)) {
                                notification.SmallIcon = null;
                            }

                            // Large Icon
                            notification.LargeIcon = reader.ReadString();
                            if (string.IsNullOrEmpty(notification.LargeIcon)) {
                                notification.LargeIcon = null;
                            }

                            result.Add(notification);
                        }

                        return result;
                    }
                }
            }
            catch (EndOfStreamException) {
                // file is broken
                return null;
            }
        }
    }
}