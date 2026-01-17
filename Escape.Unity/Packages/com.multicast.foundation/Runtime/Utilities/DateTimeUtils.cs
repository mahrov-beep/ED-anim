namespace Multicast.Utilities {
    using System;
    using JetBrains.Annotations;

    public static class DateTimeUtils {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [PublicAPI]
        public static DateTime FromUnixTime(long unixTime) {
            return Epoch.AddSeconds(unixTime);
        }
    }
}