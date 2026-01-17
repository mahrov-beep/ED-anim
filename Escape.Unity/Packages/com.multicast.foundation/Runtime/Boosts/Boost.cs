namespace Multicast.Boosts {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Multicast;
    using UniMob;

    public class Boost : ILifetimeScope {
        private readonly List<BoostData>  list;
        private readonly MutableAtom<int> version;

        internal List<BoostData> List    => this.list;
        internal Atom<int>       Version => this.version;

        public BoostOutputType OutputType { get; }
        public Lifetime        Lifetime   { get; }
        public BoostValue      Primary    { get; }

        public Boost(Lifetime lifetime, BoostOutputType outputType) {
            this.Lifetime   = lifetime;
            this.OutputType = outputType;
            this.list       = new List<BoostData>();
            this.version    = Atom.Value(lifetime, 0);
            this.Primary    = new BoostValue(this);
            
            lifetime.Register(this.Cleanup);
        }

        private void Cleanup() {
            this.list.Clear();
        }

        public IEnumerable<BoostInfo> EnumerateBootInfos() => this.list.Select(it => it.Info);

        public BoostValue CreateFilteredValue(Func<BoostInfo, bool> filter) {
            return new BoostValue(this, filter);
        }

        internal void Register(Lifetime lt, BoostData data) {
            lt.Bracket(
                () => {
                    this.list.Add(data);
                    this.version.Invalidate();
                },
                () => {
                    this.list.Remove(data);
                    this.version.Invalidate();
                }
            );
        }
    }
}