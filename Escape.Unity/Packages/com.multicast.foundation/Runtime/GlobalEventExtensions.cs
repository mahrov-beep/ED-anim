namespace Multicast {
    using System;
    using JetBrains.Annotations;
    using Scellecs.Morpeh;
    using Scellecs.Morpeh.Collections;
    using UniMob;

    public static class GlobalEventExtensions {
        [PublicAPI]
        public static void Subscribe<TData>(this Event<TData> evt, Lifetime lifetime, Action<FastList<TData>> callback)
            where TData : struct, IEventData {
            if (lifetime.IsDisposed) {
                return;
            }

            lifetime.Register(evt.Subscribe(callback));
        }

        [PublicAPI]
        public static void SubscribeEach<TData>(this Event<TData> evt, Lifetime lifetime, Action<TData> callback)
            where TData : struct, IEventData {
            if (lifetime.IsDisposed) {
                return;
            }

            lifetime.Register(evt.Subscribe(Call));

            void Call(FastList<TData> list) {
                foreach (var it in list) {
                    callback.Invoke(it);
                }
            }
        }

        [PublicAPI]
        public static void SubscribeEach<TData>(this Event<TData> evt, Lifetime lifetime, Predicate<TData> filter, Action<TData> callback)
            where TData : struct, IEventData {
            if (lifetime.IsDisposed) {
                return;
            }

            lifetime.Register(evt.Subscribe(Call));

            void Call(FastList<TData> list) {
                foreach (var it in list) {
                    if (!filter.Invoke(it)) {
                        continue;
                    }

                    callback.Invoke(it);
                }
            }
        }
    }
}