namespace Multicast.Collections {
    using System;
    using System.Collections.Generic;

    internal class SelectCache<TSource, TResult> : ICache<TResult> {
        private readonly ICache<TSource> source;
        private readonly Func<TSource, TResult> selector;

        public SelectCache(ICache<TSource> source, Func<TSource, TResult> selector) {
            this.source = source;
            this.selector = selector;
        }

        public TResult Get(string path) {
            return this.selector.Invoke(this.source.Get(path));
        }
    }

    internal class SelectEnumerableCache<TSource, TResult> : SelectCache<TSource, TResult>, IEnumerableCache<TResult> {
        private readonly IEnumerableCache<TSource> source;

        public SelectEnumerableCache(IEnumerableCache<TSource> source, Func<TSource, TResult> selector) : base(source, selector) {
            this.source = source;
        }

        public IEnumerable<string> EnumeratePaths() {
            return this.source.EnumeratePaths();
        }
    }
}