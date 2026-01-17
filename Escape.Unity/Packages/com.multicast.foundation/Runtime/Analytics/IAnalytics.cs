namespace Multicast.Analytics {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public interface IAnalytics {
        void Send([NotNull] IAnalyticsEvent evt);

        void Send(string name, params AnalyticsArg[] args);

        void Flush();
    }

    public interface IAnalyticsRegistration {
        void RegisterAdapter([NotNull] IAnalyticsAdapter adapter);

        void RegisterGlobalArgument([NotNull] Func<AnalyticsArg> argBuilder);
    }

    public sealed class AnalyticsUntypedEvent : IAnalyticsEvent {
        public AnalyticsUntypedEvent(string name, AnalyticsArgCollection args) {
            this.Name = name;
            this.Args = args;
        }

        public string                 Name { get; }
        public AnalyticsArgCollection Args { get; }
    }

    public interface IAnalyticsAdapter {
        [NotNull] string Name { get; }

        void Send(BakedAnalyticsEvent evt);

        void Flush();
    }

    public interface IAnalyticsEvent {
        [NotNull] string Name { get; }

        [NotNull] AnalyticsArgCollection Args { get; }
    }

    public readonly struct BakedAnalyticsEvent {
        public string                 Name        { get; }
        public AnalyticsArgCollection Args        { get; }
        public IAnalyticsEvent        SourceEvent { get; }

        public BakedAnalyticsEvent(string name, AnalyticsArgCollection args, IAnalyticsEvent sourceEvent) {
            this.Name        = name;
            this.Args        = args;
            this.SourceEvent = sourceEvent;
        }
    }
}