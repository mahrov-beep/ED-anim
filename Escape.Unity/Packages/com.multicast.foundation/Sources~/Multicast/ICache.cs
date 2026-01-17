namespace Multicast {
    using Multicast.Collections;
    using System;
    using System.Collections.Generic;

    public interface ICache<out T> {
        T Get(string path);
    }

    public interface IEnumerableCache<out T> : ICache<T> {
        IEnumerable<string> EnumeratePaths();
    }

    public static class CacheExtensions {
        public static ICache<TResult> Select<TSource, TResult>(this ICache<TSource> source, Func<TSource, TResult> selector) {
            return new SelectCache<TSource, TResult>(source, selector);
        }

        public static IEnumerableCache<TResult> Select<TSource, TResult>(this IEnumerableCache<TSource> source, Func<TSource, TResult> selector) {
            return new SelectEnumerableCache<TSource, TResult>(source, selector);
        }
    }
}