namespace Multicast.Numerics {
    using System;
    using System.Globalization;
    using JetBrains.Annotations;

    [Serializable]
    public struct GameTime : IEquatable<GameTime>, IComparable<GameTime> {
        public long ticks;

        [PublicAPI]
        public GameTime Date => new GameTime {
            ticks = this.AsDateTime.Date.Ticks,
        };

        public bool Equals(GameTime other) {
            return this.AsDateTime.Equals(other.AsDateTime);
        }

        public int CompareTo(GameTime other) {
            return this.AsDateTime.CompareTo(other.AsDateTime);
        }

        public override bool Equals(object obj) {
            return obj is GameTime other && this.Equals(other);
        }

        public override int GetHashCode() {
            return this.AsDateTime.GetHashCode();
        }

        public override string ToString() {
            return this.AsDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        [Obsolete("Use .AsDateTime instead")]
        public DateTime ToDateTime() => this.AsDateTime;

        [PublicAPI]
        public readonly DateTime AsDateTime => new DateTime(this.ticks, DateTimeKind.Utc);

        [PublicAPI]
        public GameTime Add(TimeSpan span) => new GameTime {
            ticks = (this.AsDateTime + span).Ticks,
        };

        [PublicAPI]
        public GameTime AddSeconds(float seconds) => this.Add(TimeSpan.FromSeconds(seconds));

        [PublicAPI]
        public static GameTime FromUtcDateTime_UNSAFE(DateTime dateTime) {
            return new GameTime { ticks = dateTime.Ticks };
        }

        [PublicAPI]
        public static bool TryParse(string format, out GameTime result) {
            if (!DateTime.TryParseExact(format, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt)) {
                result = default;
                return false;
            }

            result = new GameTime { ticks = dt.Ticks };
            return true;
        }

        public static TimeSpan operator -(GameTime a, GameTime b) {
            return a.AsDateTime - b.AsDateTime;
        }

        public static bool operator <(GameTime a, GameTime b) {
            return a.AsDateTime < b.AsDateTime;
        }

        public static bool operator >(GameTime a, GameTime b) {
            return a.AsDateTime > b.AsDateTime;
        }
    }
}