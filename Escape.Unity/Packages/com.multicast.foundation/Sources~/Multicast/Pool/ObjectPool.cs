using System;
using System.Collections.Generic;

namespace Multicast.Pool {
    internal class ObjectPool<T> : IDisposable, IObjectPool<T> where T : class {
        internal readonly List<T> list;
        private readonly Func<T> createFunc;
        private readonly Action<T> actionOnGet;
        private readonly Action<T> actionOnRelease;
        private readonly Action<T> actionOnDestroy;
        private readonly int maxSize;
        internal bool collectionCheck;

        public int CountAll { get; private set; }

        public int CountActive => this.CountAll - this.CountInactive;

        public int CountInactive => this.list.Count;

        public ObjectPool(
          Func<T> createFunc,
          Action<T> actionOnGet = null,
          Action<T> actionOnRelease = null,
          Action<T> actionOnDestroy = null,
          bool collectionCheck = true,
          int defaultCapacity = 10,
          int maxSize = 10000) {
            if (createFunc == null) {
                throw new ArgumentNullException(nameof(createFunc));
            }

            if (maxSize <= 0) {
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            }

            this.list = new List<T>(defaultCapacity);
            this.createFunc = createFunc;
            this.maxSize = maxSize;
            this.actionOnGet = actionOnGet;
            this.actionOnRelease = actionOnRelease;
            this.actionOnDestroy = actionOnDestroy;
            this.collectionCheck = collectionCheck;
        }

        public T Get() {
            T obj;
            if (this.list.Count == 0) {
                obj = this.createFunc();
                ++this.CountAll;
            }
            else {
                int index = this.list.Count - 1;
                obj = this.list[index];
                this.list.RemoveAt(index);
            }
            Action<T> actionOnGet = this.actionOnGet;
            if (actionOnGet != null) {
                actionOnGet(obj);
            }

            return obj;
        }

        public PooledObject<T> Get(out T v) => new PooledObject<T>(v = this.Get(), (IObjectPool<T>)this);

        public void Release(T element) {
            if (this.collectionCheck && this.list.Count > 0) {
                for (int index = 0; index < this.list.Count; ++index) {
                    if ((object)element == (object)this.list[index]) {
                        throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                    }
                }
            }
            Action<T> actionOnRelease = this.actionOnRelease;
            if (actionOnRelease != null) {
                actionOnRelease(element);
            }

            if (this.CountInactive < this.maxSize) {
                this.list.Add(element);
            }
            else {
                --this.CountAll;
                Action<T> actionOnDestroy = this.actionOnDestroy;
                if (actionOnDestroy != null) {
                    actionOnDestroy(element);
                }
            }
        }

        public void Clear() {
            if (this.actionOnDestroy != null) {
                foreach (T obj in this.list) {
                    this.actionOnDestroy(obj);
                }
            }
            this.list.Clear();
            this.CountAll = 0;
        }

        public void Dispose() => this.Clear();
    }
}
