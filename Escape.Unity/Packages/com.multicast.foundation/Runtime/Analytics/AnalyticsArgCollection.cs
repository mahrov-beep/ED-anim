namespace Multicast.Analytics {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class AnalyticsArgCollection : IEnumerable<AnalyticsArg> {
        private List<AnalyticsArg> args;

        public AnalyticsArgCollection() {
        }

        public bool IsEmpty => this.args == null || this.args.Count == 0;

        internal AnalyticsArgCollection(IReadOnlyCollection<AnalyticsArg> args) {
            if (args.Count != 0) {
                this.args = args.ToList();
            }
        }

        public AnalyticsArgCollection Add(string key, string val) => this.Add(new AnalyticsArg(key, val));
        public AnalyticsArgCollection Add(string key, int val)    => this.Add(new AnalyticsArg(key, val));
        public AnalyticsArgCollection Add(string key, long val)   => this.Add(new AnalyticsArg(key, val));
        public AnalyticsArgCollection Add(string key, bool val)   => this.Add(new AnalyticsArg(key, val));

        public AnalyticsArgCollection Add(AnalyticsArg arg) {
            this.args ??= new List<AnalyticsArg>();

            this.args.Add(arg);

            return this;
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