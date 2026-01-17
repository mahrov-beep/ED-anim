using System;

namespace Multicast.Pool {
    internal struct PooledObject<T> : IDisposable where T : class {
        private readonly T toReturn;
        private readonly IObjectPool<T> pool;

        public PooledObject(T value, IObjectPool<T> pool) {
            this.toReturn = value;
            this.pool = pool;
        }

        void IDisposable.Dispose() => this.pool.Release(this.toReturn);
    }
}
