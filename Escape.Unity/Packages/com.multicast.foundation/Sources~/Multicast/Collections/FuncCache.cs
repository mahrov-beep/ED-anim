namespace Multicast.Collections {
    using System;
    using System.Collections.Generic;

    public class FuncCache<T> : ICache<T> {
        private readonly Func<string, T> getter;

        public FuncCache(Func<string, T> getter) {
            this.getter = getter;
        }

        public T Get(string path) {
            return this.getter.Invoke(path);
        }
    }

    public class FuncEnumerableCache<T> : FuncCache<T>, IEnumerableCache<T> {
        private readonly Func<IEnumerable<string>> pathsGetter;

        public FuncEnumerableCache(Func<string, T> getter, Func<IEnumerable<string>> pathsGetter) : base(getter) {
            this.pathsGetter = pathsGetter;
        }

        public IEnumerable<string> EnumeratePaths() {
            return this.pathsGetter.Invoke();
        }
    }
}