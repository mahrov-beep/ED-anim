namespace Multicast.Analytics {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public struct AnalyticsArg : IEnumerable<AnalyticsArg> {
        private List<AnalyticsArg> args;

        public string  Key         { get; }
        public string  StringValue { get; }
        public int     IntValue    { get; }
        public long    LongValue   { get; }
        public bool    BoolValue   { get; }
        public ArgType Type        { get; }

        public List<AnalyticsArg> InnerArgs => this.args;

        public string Value => this.Type switch {
            ArgType.String => this.StringValue ?? string.Empty,
            ArgType.Int => this.IntValue.ToString(),
            ArgType.Long => this.LongValue.ToString(),
            ArgType.Bool => this.BoolValue ? "true" : "false",
            _ => throw new ArgumentOutOfRangeException(),
        };

        public object ObjectValue => this.Type switch {
            ArgType.String => this.StringValue ?? "",
            ArgType.Int => this.IntValue,
            ArgType.Long => this.LongValue,
            ArgType.Bool => this.BoolValue,
            _ => throw new ArgumentOutOfRangeException(),
        };

        public enum ArgType {
            String,
            Int,
            Long,
            Bool,
        }

        public AnalyticsArg([NotNull] string key, string val) {
            this.Key         = key ?? throw new ArgumentNullException(nameof(key));
            this.args        = null;
            this.StringValue = val ?? "";
            this.IntValue    = 0;
            this.LongValue   = 0;
            this.BoolValue   = false;
            this.Type        = ArgType.String;
        }

        public AnalyticsArg([NotNull] string key, int val) {
            this.Key         = key ?? throw new ArgumentNullException(nameof(key));
            this.args        = null;
            this.StringValue = null;
            this.IntValue    = val;
            this.LongValue   = 0;
            this.BoolValue   = false;
            this.Type        = ArgType.Int;
        }

        public AnalyticsArg([NotNull] string key, long val) {
            this.Key         = key ?? throw new ArgumentNullException(nameof(key));
            this.args        = null;
            this.StringValue = null;
            this.IntValue    = 0;
            this.LongValue   = val;
            this.BoolValue   = false;
            this.Type        = ArgType.Long;
        }

        public AnalyticsArg([NotNull] string key, bool val) {
            this.Key         = key ?? throw new ArgumentNullException(nameof(key));
            this.args        = null;
            this.StringValue = null;
            this.IntValue    = 0;
            this.LongValue   = 0;
            this.BoolValue   = val;
            this.Type        = ArgType.Bool;
        }

        public void Add(AnalyticsArg arg) {
            this.args ??= new List<AnalyticsArg>();

            if (string.IsNullOrEmpty(arg.Key)) {
                throw new ArgumentException("arg.key is null or empty");
            }

            this.args.Add(arg);
        }

        public IEnumerator<AnalyticsArg> GetEnumerator() {
            if (this.args == null) {
                yield break;
            }

            foreach (var arg in this.args) {
                yield return arg;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}