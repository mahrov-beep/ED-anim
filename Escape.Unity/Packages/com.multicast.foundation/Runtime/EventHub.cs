namespace Multicast {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using UniMob;

    public class EventHub<TMessageBase> where TMessageBase : class {
        private readonly List<(Action<TMessageBase> handler, HandlerFlags flags)> handlers = new();

        private uint invocationCount;
        private bool needsCleanup;

        [PublicAPI]
        public void Listen<TMessageType>(Lifetime lifetime, [NotNull] Action<TMessageType> handler)
            where TMessageType : class, TMessageBase {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            if (lifetime.IsDisposed) {
                return;
            }

            var actualHandler = this.RegisterInternal(Handle, HandlerFlags.None);
            lifetime.Register(() => this.UnregisterInternal(actualHandler));
            return;

            void Handle(TMessageBase msg) {
                if (msg is not TMessageType typedMessage || lifetime.IsDisposed) {
                    return;
                }

                handler(typedMessage);
            }
        }

        [PublicAPI]
        public UniTask<TMessageType> WaitOne<TMessageType>(Lifetime lifetime)
            where TMessageType : class, TMessageBase {
            if (lifetime.IsDisposed) {
                return UniTask.FromCanceled<TMessageType>();
            }

            NestedLifetimeDisposer? waitLifetimeDisposer = lifetime.CreateNested(out var waitLifetime);

            var tcs           = new UniTaskCompletionSource<TMessageType>();
            var actualHandler = this.RegisterInternal(Handle, HandlerFlags.DontInvokeIfAddedInAHandler);
            waitLifetime.Register(Dispose);
            return tcs.Task;

            void Dispose() {
                this.UnregisterInternal(actualHandler);
                tcs.TrySetCanceled();
            }

            void Handle(TMessageBase msg) {
                if (msg is not TMessageType typedMessage) {
                    return;
                }

                waitLifetimeDisposer?.Dispose();
                waitLifetimeDisposer = null;

                tcs.TrySetResult(typedMessage);
            }
        }

        [PublicAPI]
        public bool Raise(TMessageBase message) {
            var hadActiveHandlers = false;

            var initialInvocationCount = ++this.invocationCount;

            try {
                var initialCount = this.handlers.Count;

                for (var i = 0; i < this.handlers.Count; ++i) {
                    if (i >= initialCount) {
                        // this is a new handler; if it has a protection flag, don't call it
                        if (this.HasFlag(i, HandlerFlags.DontInvokeIfAddedInAHandler)) {
                            this.SetFlag(i, HandlerFlags.DontInvokeIfAddedInAHandler, false);
                            continue;
                        }
                    }

                    this.handlers[i].handler(message);

                    hadActiveHandlers = true;
                }
            }
            finally {
                --this.invocationCount;

                if (initialInvocationCount == 1 && this.needsCleanup) {
                    this.needsCleanup = false;
                    this.RemoveUnusedHandlers();
                }
            }

            return hadActiveHandlers;
        }

        private Action<TMessageBase> RegisterInternal(Action<TMessageBase> handler, HandlerFlags flag) {
            // if not in a handler, don't set this flag as it would ignore first nested handler
            if (this.invocationCount == 0) {
                flag &= ~HandlerFlags.DontInvokeIfAddedInAHandler;
            }

            this.handlers.Add((handler, flag));
            return handler;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool UnregisterInternal(Action<TMessageBase> handler) {
            var found = false;

            for (var i = 0; i < this.handlers.Count; ++i) {
                if (this.handlers[i].handler != handler) {
                    continue;
                }

                found = true;

                if (this.invocationCount == 0) {
                    // it's ok to compact now
                    this.handlers.RemoveAt(i);
                    --i;
                }
                else {
                    // need to wait
                    this.needsCleanup = true;
                    this.handlers[i]  = default;
                }
            }

            return found;
        }

        private void RemoveUnusedHandlers() {
            for (var i = 0; i < this.handlers.Count; ++i) {
                if (this.invocationCount == 0) {
                    this.handlers.RemoveAt(i--);
                }
                else {
                    this.handlers[i] = default;
                }
            }
        }

        private bool HasFlag(int i, HandlerFlags flag) {
            return (this.handlers[i].flags & flag) == flag;
        }

        private void SetFlag(int i, HandlerFlags flag, bool value) {
            var handler = this.handlers[i];

            if (value) {
                handler.flags |= flag;
            }
            else {
                handler.flags &= ~flag;
            }

            this.handlers[i] = handler;
        }

        [Flags]
        private enum HandlerFlags {
            None                        = 0,
            DontInvokeIfAddedInAHandler = 1 << 2,
        }
    }
}