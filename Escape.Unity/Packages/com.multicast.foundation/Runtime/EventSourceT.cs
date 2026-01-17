namespace Multicast {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using UniMob;
    
    public class EventSource<TMessage> {
        private readonly List<(Action<TMessage> handler, HandlerFlags flags)> handlers = new();

        private uint invocationCount;
        private bool needsCleanup;

        [PublicAPI]
        public void Listen(Lifetime lifetime, [NotNull] Action<TMessage> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            if (lifetime.IsDisposed) {
                return;
            }

            var actualHandler = this.RegisterInternal(handler, HandlerFlags.None);
            lifetime.Register(() => this.UnregisterInternal(actualHandler));
        }

        [PublicAPI]
        public UniTask<TMessage> WaitOne(Lifetime lifetime) {
            if (lifetime.IsDisposed) {
                return UniTask.FromCanceled<TMessage>();
            }

            NestedLifetimeDisposer? waitLifetimeDisposer = lifetime.CreateNested(out var waitLifetime);

            var tcs           = new UniTaskCompletionSource<TMessage>();
            var actualHandler = this.RegisterInternal(Handle, HandlerFlags.DontInvokeIfAddedInAHandler);
            waitLifetime.Register(Dispose);
            return tcs.Task;

            void Dispose() {
                this.UnregisterInternal(actualHandler);
                tcs.TrySetCanceled();
            }

            void Handle(TMessage msg) {
                waitLifetimeDisposer?.Dispose();
                waitLifetimeDisposer = null;

                tcs.TrySetResult(msg);
            }
        }

        [PublicAPI]
        public bool Raise(TMessage message) {
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

        private Action<TMessage> RegisterInternal(Action<TMessage> handler, HandlerFlags flag) {
            // if not in a handler, don't set this flag as it would ignore first nested handler
            if (this.invocationCount == 0) {
                flag &= ~HandlerFlags.DontInvokeIfAddedInAHandler;
            }

            this.handlers.Add((handler, flag));
            return handler;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool UnregisterInternal(Action<TMessage> handler) {
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