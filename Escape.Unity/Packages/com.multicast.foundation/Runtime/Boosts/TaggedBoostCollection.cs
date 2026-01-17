namespace Multicast.Boosts {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Numerics;

    public class TaggedBoostCollection<TKey> where TKey : struct {
        private readonly Func<BoostInfo, List<BoostTag>> tagSelector;

        private readonly Dictionary<TKey, Dictionary<BoostTag, BoostValue>> cache = new();

        public BoostCollection<TKey> Boosts { get; }

        public TaggedBoostCollection(BoostCollection<TKey> boosts, Func<BoostInfo, List<BoostTag>> tagSelector) {
            this.tagSelector = tagSelector;
            this.Boosts      = boosts;

            this.Boosts.Lifetime.Register(this.Cleanup);
        }

        private void Cleanup() {
            foreach (var dict in this.cache.Values) {
                dict.Clear();
            }

            this.cache.Clear();
        }

        [PublicAPI] public BigDouble Get(TKey key, BoostTag tag) => this.GetOrCreateBoostValue(key, tag).Value;

        [PublicAPI] public bool HasAnyBoost(TKey key, BoostTag tag) => this.GetOrCreateBoostValue(key, tag).HasAnyBoost;

        [PublicAPI]
        public BoostDetails CreateDetails(TKey key, BoostTag tag) {
            return this.GetOrCreateBoostValue(key, tag).Details;
        }

        [PublicAPI] public IEnumerable<BoostTag> EnumerateAllTags(TKey key) {
            return this.Boosts.Get(key)
                .EnumerateBootInfos()
                .SelectMany(it => this.tagSelector(it) ?? Enumerable.Empty<BoostTag>())
                .Distinct();
        }

        private BoostValue GetOrCreateBoostValue(TKey key, BoostTag tag) {
            if (!this.cache.TryGetValue(key, out var dict)) {
                this.cache[key] = dict = new Dictionary<BoostTag, BoostValue>();
            }

            if (!dict.TryGetValue(tag, out var val)) {
                dict[tag] = val = this.CreateBoostValue(key, tag);
            }

            return val;
        }

        private BoostValue CreateBoostValue(TKey key, BoostTag tag) {
            return this.Boosts.Get(key).CreateFilteredValue(Filter);

            bool Filter(BoostInfo it) {
                var matchedTags = this.tagSelector.Invoke(it);

                // wildcard boost
                if (matchedTags == null) {
                    return true;
                }

                return matchedTags.Contains(tag);
            }
        }
    }
}