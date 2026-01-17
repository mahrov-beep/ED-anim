namespace Multicast.Modules.Analytics {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Multicast.Analytics;
    using UnityEngine;
    using UnityEngine.Pool;

    internal class Analytics : IAnalytics, IAnalyticsRegistration {
        private readonly List<IAnalyticsAdapter>  adapters          = new();
        private readonly List<Func<AnalyticsArg>> globalArgBuilders = new();

        private readonly List<BakedAnalyticsEvent> pendingEvents = new();
        private readonly SynchronizationContext    synchronizationContext;

        private readonly SendOrPostCallback sendPendingEventsAction;

        private bool scheduled;

        public Analytics() {
            this.synchronizationContext  = SynchronizationContext.Current;
            this.sendPendingEventsAction = this.SendPendingEvents;
        }

        public void RegisterAdapter(IAnalyticsAdapter adapter) {
            if (adapter == null) {
                throw new ArgumentNullException(nameof(adapter));
            }

            this.adapters.Add(adapter);
        }

        public void RegisterGlobalArgument(Func<AnalyticsArg> argBuilder) {
            if (argBuilder == null) {
                throw new ArgumentNullException(nameof(argBuilder));
            }

            this.globalArgBuilders.Add(argBuilder);
        }

        public void Send(IAnalyticsEvent evt) {
            if (evt == null) {
                throw new ArgumentNullException(nameof(evt));
            }

            this.SendInternal(new BakedAnalyticsEvent(evt.Name, evt.Args, evt));
        }

        private void SendInternal(BakedAnalyticsEvent evt) {
            if (evt.Args == null) {
                throw new ArgumentException($"{evt.GetType().Name}.Args is null");
            }

            if (string.IsNullOrEmpty(evt.Name)) {
                Debug.LogError($"{evt.GetType().Name}.Name is null or empty");
                return;
            }

            foreach (var arg in evt.Args) {
                if (arg.Key == null) {
                    Debug.LogError($"{evt.GetType().Name}.Args[N].key is null");
                    return;
                }
            }

            lock (this.pendingEvents) {
                this.pendingEvents.Add(evt);

                if (!this.scheduled) {
                    this.scheduled = true;
                    this.synchronizationContext.Post(this.sendPendingEventsAction, null);
                }
            }
        }

        public void Send(string name, params AnalyticsArg[] args) {
            this.Send(new AnalyticsUntypedEvent(name, new AnalyticsArgCollection(args)));
        }

        public void Flush() {
            this.SendPendingEvents();

            foreach (var adapter in this.adapters) {
                adapter.Flush();
            }
        }

        private void SendPendingEvents(object _ = null) {
            using (ListPool<BakedAnalyticsEvent>.Get(out var tempList)) {
                lock (this.pendingEvents) {
                    this.scheduled = false;

                    foreach (var evt in this.pendingEvents) {
                        tempList.Add(evt);
                    }

                    this.pendingEvents.Clear();
                }

                for (var index = 0; index < tempList.Count; index++) {
                    var evt = tempList[index];

                    this.InjectGlobalArguments(ref evt);

                    foreach (var adapter in this.adapters) {
                        try {
                            adapter.Send(evt);
                        }
                        catch (Exception e) {
                            Debug.LogError(e);
                        }
                    }
                }
            }
        }

        private void InjectGlobalArguments(ref BakedAnalyticsEvent evt) {
            if (this.globalArgBuilders.Count == 0) {
                return;
            }

            foreach (var globalArgBuilder in this.globalArgBuilders) {
                try {
                    var injectedArg = globalArgBuilder.Invoke();
                    evt.Args.Add(injectedArg);
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }
    }
}