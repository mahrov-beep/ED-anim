namespace Multicast.Modules.TIme {
    using System;
    using GameProperties;
    using Numerics;
    using UniMob;

    internal class TimeService : ITimeService, ILifetimeScope {
        private readonly GamePropertiesModel gameProperties;

        public TimeService(Lifetime lifetime, GamePropertiesModel gameProperties) {
            this.gameProperties = gameProperties;
            this.Lifetime       = lifetime;
        }

        public Lifetime Lifetime { get; }

        [Atom] private TimeSpan CheatOffset {
            get {
                var offset = TimeSpan.Zero;
                offset += TimeSpan.FromHours(this.gameProperties.Get(TimeGameProperties.HoursOffset));
                offset += TimeSpan.FromDays(this.gameProperties.Get(TimeGameProperties.DaysOffset));
                return offset;
            }
        }

        public GameTime Now {
            get {
                Ticker.TickEveryFrame();

                return GameTime.FromUtcDateTime_UNSAFE(DateTime.UtcNow + this.CheatOffset);
            }
        }

        public bool InPast(GameTime time) {
            return time < this.Now;
        }

        public bool InFuture(GameTime time) {
            return time > this.Now;
        }
    }
}