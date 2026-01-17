namespace Multicast.UserData {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MessagePack;
    using Sirenix.OdinInspector;
    using UniMob;
    using UnityEngine.Pool;

    public sealed class UdDict<T> : UdObjectBase, IDataDict<T>, IEnumerable<T>
        where T : UdObjectBase {
        private readonly Func<UdArgs, T>        factory;
        private readonly List<ItemModification> modifications;

        [ShowInInspector, LabelText("@MyKey"), ReadOnly, EnableGUI]
        private readonly Dictionary<string, T> value;
        private readonly Dictionary<string, T> removedItems;

        public UdDict(UdArgs args, Func<UdArgs, T> factory) : base(args) {
            this.factory       = factory;
            this.modifications = new List<ItemModification>();
            this.value         = new Dictionary<string, T>();
            this.removedItems  = new Dictionary<string, T>();
        }

        string IDataObject.MyKey => this.MyKey;
        
        event Action IDataDict<T>.SelfChanged {
            add => this.SelfChanged += value;
            remove => this.SelfChanged -= value;
        }
        
        [PublicAPI]
        public T this[string key] => this.TryGetValue(key, out var item)
            ? item
            : throw new KeyNotFoundException();

        [PublicAPI]
        public int Count {
            get {
                this.Track();

                return this.value.Count;
            }
        }

        [PublicAPI]
        public T GetOrCreate(string key, out bool created) {
            if (this.TryGetValue(key, out var existing)) {
                created = false;
                return existing;
            }

            created = true;
            return this.Create(key);
        }

        [PublicAPI]
        public T Create(string key) {
            using var _ = Atom.NoWatch;

            this.AssertTransactionActive();

            if (this.ContainsKey(key)) {
                throw new ArgumentException("item with same key already exist");
            }

            this.modifications.Add(new ItemModification(ItemModificationType.Add, key));

            var item = this.factory.Invoke(new UdArgs(this, key));

            this.value.Add(key, item);
            this.removedItems.Remove(key);

            this.Invalidate();

            return item;
        }

        [PublicAPI]
        public bool Remove(T item) {
            return this.Remove(item.MyKey);
        }

        [PublicAPI]
        public bool Remove(string key) {
            using var _ = Atom.NoWatch;

            this.AssertTransactionActive();

            if (!this.value.TryGetValue(key, out var item)) {
                return false;
            }

            this.modifications.Add(new ItemModification(ItemModificationType.Remove, key));

            this.removedItems.Add(key, item);
            this.value.Remove(key);

            this.Invalidate();

            return true;
        }

        [PublicAPI]
        public bool Contains(T item) {
            return this.ContainsKey(item.MyKey);
        }

        [PublicAPI]
        public bool ContainsKey(string key) {
            this.Track();

            return this.value.ContainsKey(key);
        }

        [PublicAPI]
        public T Get(string key) {
            return this.TryGetValue(key, out var existing) ? existing : throw new KeyNotFoundException();
        }

        [PublicAPI]
        public bool TryGetValue(string key, out T item) {
            this.Track();

            return this.value.TryGetValue(key, out item);
        }

        public Dictionary<string, T>.ValueCollection.Enumerator GetEnumerator() {
            this.Track();
            return this.value.Values.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            this.Track();
            return this.value.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            this.Track();
            return this.value.Values.GetEnumerator();
        }

        internal override int TransactionVersion => this.MyParent.TransactionVersion;

        internal override bool IsTransactionActive => this.MyParent.IsTransactionActive;

        internal override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            using (ListPool<UdObjectBase>.Get(out var childrenToSerialize)) {
                foreach (var (_, child) in this.value) {
                    if (child != null) {
                        childrenToSerialize.Add(child);
                    }
                }

                writer.WriteMapHeader(childrenToSerialize.Count);

                foreach (var child in childrenToSerialize) {
                    writer.Write(child.MyKey);

                    // if (child.ShouldBeSerialized) {
                    //     child.Serialize(ref writer, options);
                    // }
                    // else {
                    //     writer.WriteNil();
                    // }
                    child.Serialize(ref writer, options);
                }
            }
        }

        internal override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            options.Security.DepthStep(ref reader);

            var count = reader.ReadMapHeader();

            for (var i = 0; i < count; i++) {
                var key = reader.ReadString();

                if (!this.value.TryGetValue(key, out var child)) {
                    child = this.factory.Invoke(new UdArgs(this, key));
                    this.value.Add(key, child);
                }

                if (!reader.TryReadNil()) {
                    child.Deserialize(ref reader, options);
                }
            }

            reader.Depth--;
        }

        internal override void RecordChangeSet(UdDataChangeSet changeSet) {
            if (this.modifications.Count > 0) {
                foreach (var m in this.modifications) {
                    T item;
                    switch (m.Type) {
                        case ItemModificationType.Add:
                            if (this.value.TryGetValue(m.Key, out item)) {
                                changeSet.Update(item);
                            }

                            break;

                        case ItemModificationType.Remove:
                            if (this.removedItems.TryGetValue(m.Key, out item)) {
                                changeSet.Delete(item);
                            }

                            break;
                    }
                }
            }

            foreach (var child in this.value.Values) {
                child.RecordChangeSet(changeSet);
            }
        }

        internal override void FlushAndClearModifications() {
            if (this.modifications.Count > 0) {
                this.removedItems.Clear();
                this.modifications.Clear();

                this.NotifySelfChanged();
            }

            foreach (var child in this.value.Values) {
                child.FlushAndClearModifications();
            }
        }

        protected override void AddChild(string key, UdObjectBase child) {
        }

        private readonly struct ItemModification {
            public readonly ItemModificationType Type;
            public readonly string               Key;

            public ItemModification(ItemModificationType type, string key) {
                this.Type = type;
                this.Key  = key;
            }
        }

        private enum ItemModificationType {
            Add,
            Remove,
        }
    }
}