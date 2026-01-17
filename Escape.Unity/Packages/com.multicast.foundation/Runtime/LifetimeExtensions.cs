namespace Multicast {
    using System;
    using JetBrains.Annotations;
    using UniMob;

    public static class LifetimeExtensions {
        [PublicAPI]
        public static void Bracket(this Lifetime lifetime,
            [NotNull] Action opening, [NotNull] Action closing) {
            if (opening == null) {
                throw new ArgumentNullException(nameof(opening));
            }

            if (closing == null) {
                throw new ArgumentNullException(nameof(closing));
            }

            if (lifetime.IsDisposed) {
                return;
            }

            lifetime.Register(closing);
            opening.Invoke();
        }

        [PublicAPI]
        public static void Bracket<T>(this Lifetime lifetime,
            [NotNull] Action<T> opening, [NotNull] Action<T> closing, T state) {
            if (opening == null) {
                throw new ArgumentNullException(nameof(opening));
            }

            if (closing == null) {
                throw new ArgumentNullException(nameof(closing));
            }

            if (lifetime.IsDisposed) {
                return;
            }

            lifetime.Register(() => closing.Invoke(state));
            opening.Invoke(state);
        }

        [PublicAPI]
        public static void RegisterToEvent<T>(this Lifetime lifetime,
            [NotNull] Action<T> subscribe, [NotNull] Action<T> unsubscribe, T arg) {
            lifetime.Bracket(subscribe, unsubscribe, arg);
        }
    }
}