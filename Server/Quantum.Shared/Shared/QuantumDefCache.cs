namespace Quantum {
    using System;
    using System.Collections.Generic;

    public class QuantumDefCache<TKey, TDef>
        where TKey : class
        where TDef : class {
        private readonly Dictionary<TKey, TDef> cache;
        private readonly Func<TKey, TDef>       getter;

        public QuantumDefCache(Func<TKey, TDef> getter, int capacity = 16) {
            this.getter = getter;
            this.cache  = new Dictionary<TKey, TDef>(capacity);
        }

        public TDef Get(TKey key) {
            if (this.cache.TryGetValue(key, out var def)) {
                return def;
            }

            def = this.getter(key);
            this.cache.Add(key, def);
            return def;
        }
    }
}