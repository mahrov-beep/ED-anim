namespace Multicast.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    public class LookupCollection<T> : IEnumerable<KeyValuePair<string, T>>
        where T : Def {
        private readonly Dictionary<string, T> lookup;

        public List<T> Items { get; }
        public string[] Keys { get; }

        public LookupCollection(List<T> items) {
            this.Items = items;
            this.Keys = items.Select(it => it.key).ToArray();
            this.lookup = new Dictionary<string, T>(this.Items.Count);

            foreach (var item in this.Items) {
                if (this.lookup.ContainsKey(item.key)) {
                    throw new ArgumentException($"LookupCollection<{typeof(T).Name}> cannot be built due to duplicate key '{item.key}'");
                }

                this.lookup.Add(item.key, item);
            }
        }

        [PublicAPI]
        public T Get(string key) {
            if (!this.lookup.TryGetValue(key, out var result)) {
                throw new InvalidOperationException($"No item with key '{key}' of type '{typeof(T).Name}'");
            }

            return result;
        }

        [PublicAPI]
        public bool TryGet(string key, out T item) {
            return this.lookup.TryGetValue(key, out item);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() {
            return this.lookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public struct Writer {
            private readonly LookupCollection<T> collection;

            public Writer(LookupCollection<T> collection) {
                this.collection = collection;
            }

            public void Add(T item) {
                this.collection.Items.Add(item);
                this.collection.lookup.Add(item.key, item);
            }

            public void AddRange(IEnumerable<T> items) {
                foreach (var item in items) {
                    this.Add(item);
                }
            }
        }
    }
}