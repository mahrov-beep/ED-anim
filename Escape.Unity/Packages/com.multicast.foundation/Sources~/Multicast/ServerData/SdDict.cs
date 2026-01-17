using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Multicast.ServerData {
    using JetBrains.Annotations;

    public class SdDict<T> : SdObjectBase, IDataDict<T>, IEnumerable<T>
        where T : SdObjectBase {
        private readonly Func<SdArgs, T> factory;
        private readonly Dictionary<string, T> value;

        public SdDict(SdArgs args, [NotNull] Func<SdArgs, T> factory) : base(args) {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.value   = new Dictionary<string, T>();
        }

        string IDataObject.MyKey => this.GetSdObjectKey();

        public event Action SelfChanged;

        public T this[string key] => this.TryGetValue(key, out var item)
            ? item
            : throw new KeyNotFoundException();

        public int Count {
            get {
                this.TrackRead();
                return this.value.Count;
            }
        }

        public T GetOrCreate([NotNull] string key, out bool created) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            if (this.TryGetValue(key, out var existing)) {
                created = false;
                return existing;
            }

            created = true;
            return this.Create(key);
        }

        public T Create([NotNull] string key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.AssertOnWrite();

            if (this.ContainsKey(key)) {
                throw new ArgumentException("item with same key already exist");
            }

            var item = this.factory.Invoke(this.ChildInternal(key));

            this.value.Add(key, item);
            this.TrackWrite();
            this.SelfChanged?.Invoke();
            return item;
        }

        public bool Remove([NotNull] T item) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }

            return this.Remove(item.GetSdObjectKey());
        }

        public bool Remove([NotNull] string key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.AssertOnWrite();

            if (!this.value.TryGetValue(key, out var item)) {
                return false;
            }

            this.value.Remove(key);
            this.TrackWrite();
            this.SelfChanged?.Invoke();
            return true;
        }

        public bool Contains([NotNull] T item) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }

            return this.ContainsKey(item.GetSdObjectKey());
        }

        public bool ContainsKey([NotNull] string key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.TrackRead();
            return this.value.ContainsKey(key);
        }

        public T Get([NotNull] string key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            return this.TryGetValue(key, out var existing) ? existing : throw new KeyNotFoundException();
        }

        public bool TryGetValue([NotNull] string key, out T item) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.TrackRead();
            return this.value.TryGetValue(key, out item);
        }

        public Dictionary<string, T>.ValueCollection.Enumerator GetEnumerator() {
            this.TrackRead();
            return this.value.Values.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            this.TrackRead();
            return this.value.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            this.TrackRead();
            return this.value.Values.GetEnumerator();
        }

        internal override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            writer.WriteMapHeader(this.value.Count);

            foreach (var (_, child) in this.value) {
                writer.Write(child.GetSdObjectKey());
                child.Serialize(ref writer, options);
            }
        }

        internal override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            options.Security.DepthStep(ref reader);

            var newDictionary = new Dictionary<string, T>();

            var modified = false;
            var count = reader.ReadMapHeader();

            for (var i = 0; i < count; i++) {
                var key = reader.ReadString() ?? string.Empty;

                T newValue;
                if (this.value.TryGetValue(key, out var oldValue)) {
                    newValue = oldValue;
                }
                else {
                    modified = true;
                    newValue = this.factory.Invoke(this.ChildInternal(key));
                }

                newDictionary.Add(key, newValue);

                if (!reader.TryReadNil()) {
                    newValue.Deserialize(ref reader, options);
                }
            }

            var deletedKeys = this.value.Keys.Where(it => !newDictionary.ContainsKey(it)).ToList();

            foreach (var key in deletedKeys) {
                this.value.Remove(key);
                modified = true;
            }

            foreach (var (key, newValue) in newDictionary) {
                this.value[key] = newValue;
            }

            reader.Depth--;

            if (modified) {
                TrackWrite();
                this.SelfChanged?.Invoke();
            }
        }

        protected override void AddChild(SdKey key, SdObjectBase child) {
        }
    }
}